#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class TypeMapper : ExportedTypeResolverBase
    {
        private readonly PomonaConfigurationBase configuration;
        private readonly ITypeMappingFilter filter;
        private readonly HashSet<Type> sourceTypes;
        private readonly Dictionary<string, TypeSpec> typeNameMap;
        // TODO: These will be removed along with tight bidirectional coupling between TypeMapper and PomonaSessionFactory, this will break API compability.
        private IPomonaSessionFactory sessionFactory;


        public TypeMapper(PomonaConfigurationBase configuration)
            : this(configuration.CreateMappingFilter(), configuration.SourceTypes, configuration.OnMappingComplete)
        {
            this.configuration = configuration;
        }


        public TypeMapper(ITypeMappingFilter filter, IEnumerable<Type> sourceTypes, Action<TypeMapper> onMappingComplete)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");
            if (sourceTypes == null)
                throw new ArgumentNullException("sourceTypes");

            this.filter = filter;

            this.sourceTypes = new HashSet<Type>(sourceTypes.Where(this.filter.TypeIsMapped));

            this.typeNameMap = this.sourceTypes.Select(FromType).ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            if (onMappingComplete != null)
                onMappingComplete(this);
        }


        public IEnumerable<EnumTypeSpec> EnumTypes
        {
            get { return TypeMap.Values.OfType<EnumTypeSpec>(); }
        }

        public ITypeMappingFilter Filter
        {
            get { return this.filter; }
        }

        public IEnumerable<TypeSpec> SourceTypes
        {
            get { return this.sourceTypes.Select(FromType); }
        }

        internal IPomonaSessionFactory SessionFactory
        {
            get { return this.sessionFactory ?? (this.sessionFactory = this.configuration.CreateSessionFactory(this)); }
            set { this.sessionFactory = value; }
        }


        public override TypeSpec FromType(string typeName)
        {
            TypeSpec typeSpec;
            if (!TryGetTypeByName(typeName, out typeSpec))
                throw new UnknownTypeException("Type with name " + typeName + " not recognized.");
            return typeSpec;
        }


        public override bool TryGetTypeByName(string typeName, out TypeSpec typeSpec)
        {
            return base.TryGetTypeByName(typeName, out typeSpec) || this.typeNameMap.TryGetValue(typeName, out typeSpec);
        }


        public override TypeSpec LoadBaseType(TypeSpec typeSpec)
        {
            if (typeSpec is StructuredType)
            {
                if (this.filter.IsIndependentTypeRoot(typeSpec))
                    return null;

                var exposedBaseType = typeSpec.Type.BaseType;

                while (exposedBaseType != null && !this.filter.TypeIsMapped(exposedBaseType))
                    exposedBaseType = exposedBaseType.BaseType;

                if (exposedBaseType != null)
                    return FromType(exposedBaseType);
                return null;
            }
            return base.LoadBaseType(typeSpec);
        }


        public override ConstructorSpec LoadConstructor(TypeSpec typeSpec)
        {
            // TODO: Maybe introduce a IMappedType/IExportedType for this purpose
            if (typeSpec is ResourceType || typeSpec is ComplexType)
                return this.filter.GetTypeConstructor(typeSpec);
            return base.LoadConstructor(typeSpec);
        }


        public override IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec)
        {
            var attrs = base.LoadDeclaredAttributes(memberSpec);

            var typeSpec = memberSpec as TypeSpec;
            if (typeSpec != null)
            {
                var customClientLibraryType = this.filter.GetClientLibraryType(typeSpec.Type);
                if (customClientLibraryType != null)
                    attrs = attrs.Append(new CustomClientLibraryTypeAttribute(customClientLibraryType));
                var customJsonConverter = this.filter.GetJsonConverterForType(typeSpec.Type);
                if (customJsonConverter != null)
                    attrs = attrs.Append(new CustomJsonConverterAttribute(customJsonConverter));
            }
            var propSpec = memberSpec as PropertySpec;
            if (propSpec != null)
            {
                var formulaExpr = this.filter.GetPropertyFormula(propSpec.ReflectedType, propSpec.PropertyInfo);
                if (formulaExpr != null)
                    attrs = attrs.Append(new PropertyFormulaAttribute(formulaExpr));

                attrs =
                    attrs.Concat(
                        this.filter.GetPropertyAttributes(propSpec.ReflectedType, propSpec.PropertyInfo).EmptyIfNull());
            }
            return attrs;
        }


        public override TypeSpec LoadDeclaringType(PropertySpec propertySpec)
        {
            if (propertySpec is StructuredProperty)
                return FromType(GetKnownDeclaringType(propertySpec.ReflectedType, propertySpec.PropertyInfo));
            return base.LoadDeclaringType(propertySpec);
        }


        public override Func<object, IContainer, object> LoadGetter(PropertySpec propertySpec)
        {
            return this.filter.GetPropertyGetter(propertySpec.ReflectedType, propertySpec.PropertyInfo)
                   ?? base.LoadGetter(propertySpec);
        }


        public override IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec)
        {
            if (typeSpec is StructuredType)
                return base.LoadInterfaces(typeSpec).Where(x => this.filter.TypeIsMappedAsTransformedType(x));

            return base.LoadInterfaces(typeSpec);
        }


        public override string LoadName(MemberSpec memberSpec)
        {
            return memberSpec
                .Maybe()
                .Switch()
                .Case<PropertySpec>().Then(x => this.filter.GetPropertyMappedName(x.ReflectedType, x.PropertyInfo))
                .Case<TypeSpec>().Then(x => this.filter.GetTypeMappedName(x.Type))
                .EndSwitch()
                .OrDefault(() => base.LoadName(memberSpec));
        }


        public override IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec)
        {
            if (typeSpec is ComplexType || typeSpec is ResourceType)
            {
                var propertiesFromNonMappedInterfaces = typeSpec.Type.IsInterface
                    ? typeSpec.Type.GetInterfaces().Where(x => !this.filter.TypeIsMapped(x)).SelectMany(
                        x => this.filter.GetAllPropertiesOfType(x, BindingFlags.Instance | BindingFlags.Public))
                    : Enumerable.Empty<PropertyInfo>();

                return this.filter.GetAllPropertiesOfType(typeSpec,
                                                          BindingFlags.Instance | BindingFlags.Static
                                                          | BindingFlags.Public
                                                          | BindingFlags.NonPublic)
                           .Concat(propertiesFromNonMappedInterfaces)
                           .Where(x => this.filter.PropertyIsIncluded(typeSpec.Type, x))
                           .Select(x => WrapProperty(typeSpec, x));
            }

            return base.LoadProperties(typeSpec);
        }


        public override PropertyFlags LoadPropertyFlags(PropertySpec propertySpec)
        {
            return this.filter.GetPropertyFlags(propertySpec.PropertyInfo) ?? base.LoadPropertyFlags(propertySpec);
        }


        public override TypeSpec LoadPropertyType(PropertySpec propertySpec)
        {
            var complexProperty = propertySpec as StructuredProperty;
            if (complexProperty != null)
                return FromType(this.filter.GetPropertyType(complexProperty.ReflectedType, complexProperty.PropertyInfo));
            return base.LoadPropertyType(propertySpec);
        }


        public override ResourcePropertyDetails LoadResourcePropertyDetails(ResourceProperty property)
        {
            var propInfo = property.PropertyInfo;
            return new ResourcePropertyDetails(this.filter.ClientPropertyIsExposedAsRepository(propInfo),
                                               NameUtils.ConvertCamelCaseToUri(this.filter.GetPropertyMappedName(property.ReflectedType,
                                                                                                                 propInfo)));
        }


        public override ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType)
        {
            var type = resourceType.Type;
            var parentToChildProperty = this.filter.GetParentToChildProperty(type);
            var childToParentProperty = this.filter.GetChildToParentProperty(type);
            var isRootResource = parentToChildProperty == null;

            var relativeResourcePath = isRootResource ? this.filter.GetUrlRelativePath(type).TrimStart('/') : null;

            return new ResourceTypeDetails(resourceType,
                                           relativeResourcePath,
                                           this.filter.TypeIsExposedAsRepository(type),
                                           this.filter.GetPostReturnType(type),
                                           parentToChildProperty,
                                           childToParentProperty,
                                           this.filter.TypeIsSingletonResource(type),
                                           this.filter.GetResourceHandlers(type),
                                           this.filter.GetPluralNameForType(type));
        }


        public override Action<object, object, IContainer> LoadSetter(PropertySpec propertySpec)
        {
            return this.filter.GetPropertySetter(propertySpec.ReflectedType, propertySpec.PropertyInfo)
                   ?? base.LoadSetter(propertySpec);
        }


        public override StructuredPropertyDetails LoadStructuredPropertyDetails(StructuredProperty property)
        {
            var propInfo = property.PropertyInfo;

            var reflectedType = property.ReflectedType;
            var expandMode = this.filter.GetPropertyExpandMode(reflectedType, propInfo);
            var accessMode = this.filter.GetPropertyAccessMode(propInfo, property.DeclaringType.Constructor);

            var details = new StructuredPropertyDetails(
                this.filter.PropertyIsAttributes(reflectedType, propInfo),
                this.filter.PropertyIsEtag(reflectedType, propInfo),
                this.filter.PropertyIsPrimaryId(reflectedType, propInfo),
                accessMode.HasFlag(HttpMethod.Get),
                accessMode,
                this.filter.GetPropertyItemAccessMode(reflectedType, propInfo),
                expandMode);
            return details;
        }


        public override StructuredTypeDetails LoadStructuredTypeDetails(StructuredType structuredType)
        {
            // TODO: Get allowed methods from filter
            var allowedMethods = HttpMethod.Get |
                                 (this.filter.PatchOfTypeIsAllowed(structuredType) ? HttpMethod.Patch : 0) |
                                 (this.filter.PostOfTypeIsAllowed(structuredType) ? HttpMethod.Post : 0) |
                                 (this.filter.DeleteOfTypeIsAllowed(structuredType) ? HttpMethod.Delete : 0);

            var type = structuredType.Type;
            var details = new StructuredTypeDetails(structuredType,
                                                    allowedMethods,
                                                    this.filter.GetOnDeserializedHook(type),
                                                    this.filter.TypeIsMappedAsValueObject(type),
                                                    this.filter.TypeIsMappedAsValueObject(type),
                                                    this.filter.GetTypeIsAbstract(type));

            return details;
        }


        public override IEnumerable<StructuredType> LoadSubTypes(StructuredType baseType)
        {
            return TypeMap.Values.OfType<StructuredType>()
                          .Where(x => x.BaseType == baseType)
                          .SelectMany(x => x.SubTypes.Append(x))
                          .ToList();
        }


        public override ResourceType LoadUriBaseType(ResourceType resourceType)
        {
            Type uriBaseType = this.filter.GetUriBaseType(resourceType.Type);
            return uriBaseType != null ? (ResourceType)FromType(uriBaseType) : null;
        }


        public bool TryGetTypeSpec(Type type, out TypeSpec typeSpec)
        {
            typeSpec = null;
            if (!Filter.TypeIsMapped(type))
                return false;
            typeSpec = FromType(type);
            return true;
        }


        protected override TypeSpec CreateType(Type type)
        {
            if (!this.filter.TypeIsMappedAsSharedType(type) && this.filter.TypeIsMappedAsTransformedType(type))
            {
                if (this.filter.TypeIsMappedAsValueObject(type))
                    return new ComplexType(this, type);
                return new ResourceType(this, type);
            }
            return base.CreateType(type);
        }


        protected override sealed Type MapExposedClrType(Type type)
        {
            return this.filter.ResolveRealTypeForProxy(type);
        }


        private Type GetKnownDeclaringType(Type reflectedType, PropertyInfo propertyInfo)
        {
            var propBaseDefinition = propertyInfo.GetBaseDefinition();
            return reflectedType.GetFullTypeHierarchy()
                                .Where(x => propBaseDefinition.DeclaringType != null
                                            && propBaseDefinition.DeclaringType.IsAssignableFrom(x))
                                .TakeUntil(x => this.filter.IsIndependentTypeRoot(x))
                                .LastOrDefault(x => this.sourceTypes.Contains(x))
                   ?? propBaseDefinition.DeclaringType;
        }
    }
}