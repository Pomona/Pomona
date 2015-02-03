#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using Pomona.FluentMapping;
using Pomona.Routing;

namespace Pomona
{
    public class TypeMapper : ExportedTypeResolverBase, ITypeMapper
    {
        private readonly InternalRouteActionResolver actionResolver;
        private readonly PomonaConfigurationBase configuration;

        private readonly ITypeMappingFilter filter;
        private readonly PomonaRouteResolver routeResolver;
        private readonly HashSet<Type> sourceTypes;
        private readonly Dictionary<string, TypeSpec> typeNameMap;


        public TypeMapper(PomonaConfigurationBase configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            this.configuration = configuration;

            var innerFilter = configuration.TypeMappingFilter;
            var fluentRuleObjects = configuration.FluentRuleObjects.ToArray();
            this.filter = new FluentTypeMappingFilter(innerFilter, fluentRuleObjects, null, configuration.SourceTypes);
            var wrappableFilter = innerFilter as IWrappableTypeMappingFilter;
            if (wrappableFilter != null)
                wrappableFilter.BaseFilter = filter;

            if (this.filter == null)
                throw new ArgumentNullException("filter");

            this.sourceTypes = new HashSet<Type>(this.configuration.SourceTypes.Where(this.filter.TypeIsMapped));

            this.typeNameMap = new Dictionary<string, TypeSpec>();

            foreach (var sourceType in this.sourceTypes.Concat(TypeUtils.GetNativeTypes()))
            {
                var type = GetClassMapping(sourceType);
                this.typeNameMap[type.Name.ToLower()] = type;
            }

            configuration.OnMappingComplete(this);

            this.routeResolver = new PomonaRouteResolver(new DataSourceRootRoute(this));
            this.actionResolver = new InternalRouteActionResolver(configuration.RouteActionResolvers);
        }


        public PomonaConfigurationBase Configuration
        {
            get { return this.configuration; }
        }

        public IEnumerable<EnumTypeSpec> EnumTypes
        {
            get { return TypeMap.Values.OfType<EnumTypeSpec>(); }
        }

        public ITypeMappingFilter Filter
        {
            get { return this.filter; }
        }

        public ICollection<Type> SourceTypes
        {
            get { return this.sourceTypes; }
        }

        public IEnumerable<TransformedType> TransformedTypes
        {
            get { return TypeMap.Values.OfType<TransformedType>(); }
        }

        internal IRouteActionResolver ActionResolver
        {
            get { return this.actionResolver; }
        }

        internal PomonaRouteResolver RouteResolver
        {
            get { return this.routeResolver; }
        }


        public override IEnumerable<TransformedType> GetAllTransformedTypes()
        {
            return TransformedTypes;
        }


        public override TypeSpec LoadBaseType(TypeSpec typeSpec)
        {
            if (typeSpec is TransformedType)
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
            var transformedType = typeSpec as TransformedType;
            if (transformedType != null)
                return this.filter.GetTypeConstructor(transformedType);
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
                var formulaExpr = this.filter.GetPropertyFormula(propSpec.ReflectedType, propSpec.PropertyInfo)
                                  ?? (this.filter.PropertyFormulaIsDecompiled(propSpec.ReflectedType,
                                                                              propSpec.PropertyInfo)
                                      ? this.filter.GetDecompiledPropertyFormula(propSpec.ReflectedType,
                                                                                 propSpec.PropertyInfo)
                                      : null);
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
            if (propertySpec is PropertyMapping)
                return FromType(GetKnownDeclaringType(propertySpec.ReflectedType, propertySpec.PropertyInfo));
            return base.LoadDeclaringType(propertySpec);
        }


        public override ExportedPropertyDetails LoadExportedPropertyDetails(PropertyMapping propertyMapping)
        {
            var propInfo = propertyMapping.PropertyInfo;

            var reflectedType = propertyMapping.ReflectedType;
            var expandMode = filter.GetPropertyExpandMode(reflectedType, propInfo);

            var details = new ExportedPropertyDetails(
                this.filter.PropertyIsAttributes(reflectedType, propInfo),
                this.filter.PropertyIsEtag(reflectedType, propInfo),
                this.filter.PropertyIsPrimaryId(reflectedType, propInfo),
                this.filter.GetPropertyAccessMode(propInfo, propertyMapping.DeclaringType.Constructor),
                this.filter.GetPropertyItemAccessMode(reflectedType, propInfo),
                this.filter.ClientPropertyIsExposedAsRepository(propInfo),
                NameUtils.ConvertCamelCaseToUri(this.filter.GetPropertyMappedName(reflectedType,
                                                                                  propInfo)),
                expandMode);
            return details;
        }


        public override ExportedTypeDetails LoadExportedTypeDetails(TransformedType exportedType)
        {
            // TODO: Get allowed methods from filter
            var allowedMethods = HttpMethod.Get |
                                 (this.filter.PatchOfTypeIsAllowed(exportedType) ? HttpMethod.Patch : 0) |
                                 (this.filter.PostOfTypeIsAllowed(exportedType) ? HttpMethod.Post : 0) |
                                 (this.filter.DeleteOfTypeIsAllowed(exportedType) ? HttpMethod.Delete : 0);

            var type = exportedType.Type;
            var details = new ExportedTypeDetails(exportedType,
                                                  allowedMethods,
                                                  this.filter.GetPluralNameForType(type),
                                                  this.filter.GetOnDeserializedHook(type),
                                                  this.filter.TypeIsMappedAsValueObject(type),
                                                  this.filter.TypeIsMappedAsValueObject(type),
                                                  this.filter.GetTypeIsAbstract(type));

            return details;
        }


        public override Func<object, IContainer, object> LoadGetter(PropertySpec propertySpec)
        {
            return this.filter.GetPropertyGetter(propertySpec.ReflectedType, propertySpec.PropertyInfo)
                   ?? base.LoadGetter(propertySpec);
        }


        public override IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec)
        {
            if (typeSpec is TransformedType)
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
            if (typeSpec is TransformedType)
            {
                var propertiesFromNonMappedInterfaces = typeSpec.Type.IsInterface
                    ? typeSpec.Type.GetInterfaces().Where(x => !this.filter.TypeIsMapped(x)).SelectMany(
                        x => x.GetProperties())
                    : Enumerable.Empty<PropertyInfo>();

                return typeSpec.Type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
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
            var propMapping = propertySpec as PropertyMapping;
            if (propMapping != null)
                return FromType(this.filter.GetPropertyType(propMapping.ReflectedType, propMapping.PropertyInfo));
            return base.LoadPropertyType(propertySpec);
        }


        public override ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType)
        {
            var type = resourceType.Type;
            var parentToChildProperty = this.filter.GetParentToChildProperty(type);
            var childToParentProperty = this.filter.GetChildToParentProperty(type);
            var isRootResource = parentToChildProperty == null;

            var relativeResourcePath = isRootResource ? this.filter.GetUrlRelativePath(type) : null;

            return new ResourceTypeDetails(resourceType,
                                           relativeResourcePath,
                                           this.filter.TypeIsExposedAsRepository(type),
                                           this.filter.GetPostReturnType(type),
                                           parentToChildProperty,
                                           childToParentProperty,
                                           this.filter.GetResourceHandlers(type));
        }


        public override Action<object, object, IContainer> LoadSetter(PropertySpec propertySpec)
        {
            return this.filter.GetPropertySetter(propertySpec.ReflectedType, propertySpec.PropertyInfo)
                   ?? base.LoadSetter(propertySpec);
        }


        public override ResourceType LoadUriBaseType(ResourceType resourceType)
        {
            Type uriBaseType = this.filter.GetUriBaseType(resourceType.Type);
            return uriBaseType != null ? (ResourceType)FromType(uriBaseType) : null;
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return FromType(type);
        }


        public TypeSpec GetClassMapping(string typeName)
        {
            TypeSpec type;
            if (!this.typeNameMap.TryGetValue(typeName.ToLower(), out type))
                throw new UnknownTypeException("Type with name " + typeName + " not recognized.");
            return type;
        }


        public TypeSpec GetClassMapping<T>()
        {
            var type = typeof(T);

            return GetClassMapping(type);
        }


        public bool TryGetTypeSpec(Type type, out TypeSpec typeSpec)
        {
            typeSpec = null;
            if (!Filter.TypeIsMapped(type))
                return false;
            typeSpec = GetClassMapping(type);
            return true;
        }


        protected override TypeSpec CreateType(Type type)
        {
            if (!this.filter.TypeIsMappedAsSharedType(type) && this.filter.TypeIsMappedAsTransformedType(type))
            {
                if (this.filter.TypeIsMappedAsValueObject(type))
                    return new TransformedType(this, type);
                return new ResourceType(this, type);
            }
            return base.CreateType(type);
        }


        protected override sealed Type MapExposedClrType(Type type)
        {
            return this.filter.ResolveRealTypeForProxy(type);
        }


        private TypeSpec CreateClassMapping(Type type)
        {
            return FromType(type);
        }


        private Type GetKnownDeclaringType(Type reflectedType, PropertyInfo propertyInfo)
        {
            // Hack, IGrouping
            var propBaseDefinition = propertyInfo.GetBaseDefinition();
            return reflectedType.GetFullTypeHierarchy()
                .Where(x => propBaseDefinition.DeclaringType.IsAssignableFrom(x))
                .TakeUntil(x => this.filter.IsIndependentTypeRoot(x))
                .LastOrDefault(x => SourceTypes.Contains(x)) ??
                   propBaseDefinition.DeclaringType;
        }
    }
}