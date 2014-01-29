#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;

namespace Pomona
{
    public class TypeMapper : ExportedTypeResolverBase, ITypeMapper
    {
        private readonly PomonaConfigurationBase configuration;
        private readonly ITypeMappingFilter filter;
        private readonly Dictionary<Type, TypeSpec> mappings = new Dictionary<Type, TypeSpec>();
        private readonly ISerializerFactory serializerFactory;
        private readonly HashSet<Type> sourceTypes;
        private readonly Dictionary<string, TypeSpec> typeNameMap;


        public TypeMapper(PomonaConfigurationBase configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            this.configuration = configuration;

            this.filter = configuration.TypeMappingFilter;
            var fluentRuleObjects = configuration.FluentRuleObjects.ToArray();
            if (fluentRuleObjects.Length > 0)
                this.filter = new FluentTypeMappingFilter(this.filter, fluentRuleObjects, null, configuration.SourceTypes);

            if (this.filter == null)
                throw new ArgumentNullException("filter");

            this.sourceTypes = new HashSet<Type>(this.configuration.SourceTypes.Where(this.filter.TypeIsMapped));

            this.typeNameMap = new Dictionary<string, TypeSpec>();

            foreach (var sourceType in this.sourceTypes.Concat(TypeUtils.GetNativeTypes()))
            {
                var type = GetClassMapping(sourceType);
                this.typeNameMap[type.Name.ToLower()] = type;
            }

            this.serializerFactory = configuration.SerializerFactory;

            configuration.OnMappingComplete(this);
        }


        public IEnumerable<EnumTypeSpec> EnumTypes
        {
            get { return this.mappings.Values.OfType<EnumTypeSpec>(); }
        }

        public ITypeMappingFilter Filter
        {
            get { return this.filter; }
        }

        /// <summary>
        /// The Json serializer factory.
        /// TODO: This should be moved out of here..
        /// </summary>
        public ISerializerFactory SerializerFactory
        {
            get { return this.serializerFactory; }
        }

        public ICollection<Type> SourceTypes
        {
            get { return this.sourceTypes; }
        }

        public IEnumerable<TransformedType> TransformedTypes
        {
            get { return this.mappings.Values.OfType<TransformedType>(); }
        }


        public override IEnumerable<TransformedType> GetAllTransformedTypes()
        {
            return TransformedTypes;
        }


        public override ExportedPropertyDetails LoadExportedPropertyDetails(PropertyMapping propertyMapping)
        {
            var propInfo = propertyMapping.PropertyInfo;
            var details = new ExportedPropertyDetails(
                this.filter.PropertyIsAttributes(propInfo),
                this.filter.PropertyIsEtag(propInfo),
                this.filter.PropertyIsPrimaryId(propInfo),
                this.filter.GetPropertyAccessMode(propInfo, propertyMapping.DeclaringType.Constructor),
                this.filter.GetPropertyItemAccessMode(propInfo),
                this.filter.ClientPropertyIsExposedAsRepository(propInfo),
                NameUtils.ConvertCamelCaseToUri(this.filter.GetPropertyMappedName(propInfo)),
                this.filter.PropertyIsAlwaysExpanded(propInfo));
            return details;
        }


        public override ConstructorSpec LoadConstructor(TypeSpec typeSpec)
        {
            var transformedType = typeSpec as TransformedType;
            if (transformedType != null)
            {
                return this.filter.GetTypeConstructor(transformedType);
            }
            return base.LoadConstructor(typeSpec);
        }


        public override ExportedTypeDetails LoadExportedTypeDetails(TransformedType exportedType)
        {
            // TODO: Get allowed methods from filter
            var allowedMethods = HttpMethod.Get | (filter.PatchOfTypeIsAllowed(exportedType) ? HttpMethod.Patch : 0)
                             | (filter.PostOfTypeIsAllowed(exportedType) ? HttpMethod.Post : 0);

            var type = exportedType.Type;
            var details = new ExportedTypeDetails(exportedType,
                allowedMethods,
                this.filter.GetPluralNameForType(type),
                this.filter.GetOnDeserializedHook(type),
                this.filter.TypeIsMappedAsValueObject(type),
                this.filter.TypeIsMappedAsValueObject(type));

            return details;
        }


        public override IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec)
        {
            var attrs = base.LoadDeclaredAttributes(memberSpec);

            var typeSpec = memberSpec as TypeSpec;
            if (typeSpec != null)
            {
                var customClientLibraryType = filter.GetClientLibraryType(typeSpec.Type);
                if (customClientLibraryType != null)
                {
                    attrs = attrs.Concat(new CustomClientLibraryTypeAttribute(customClientLibraryType));
                }
                var customJsonConverter = filter.GetJsonConverterForType(typeSpec.Type);
                if (customJsonConverter != null)
                {
                    attrs = attrs.Concat(new CustomJsonConverterAttribute(customJsonConverter));
                }
            }
            var propSpec = memberSpec as PropertySpec;
            if (propSpec != null)
            {
                var formulaExpr = filter.GetPropertyFormula(propSpec.PropertyInfo)
                                  ?? (filter.PropertyFormulaIsDecompiled(propSpec.PropertyInfo)
                                      ? filter.GetDecompiledPropertyFormula(propSpec.PropertyInfo)
                                      : null);
                if (formulaExpr != null)
                {
                    attrs = attrs.Concat(new PropertyFormulaAttribute(formulaExpr));
                }
            }
            return attrs;
        }


        public override string LoadName(MemberSpec memberSpec)
        {
            return
                memberSpec.Maybe().OfType<PropertySpec>().Select(x => this.filter.GetPropertyMappedName(x.PropertyInfo))
                    .OrDefault(() => base.LoadName(memberSpec));
        }


        public override IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec)
        {
            if (typeSpec is TransformedType)
            {
                return typeSpec.Type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                   | BindingFlags.NonPublic).Where(x => this.filter.PropertyIsIncluded(x))
                    .Select(x => WrapProperty(typeSpec, x));
            }

            return base.LoadProperties(typeSpec);
        }


        public override TypeSpec LoadBaseType(TypeSpec typeSpec)
        {
            if (typeSpec is TransformedType)
            {
                if (filter.IsIndependentTypeRoot(typeSpec))
                    return null;

                var exposedBaseType = typeSpec.Type.BaseType;

                while (exposedBaseType != null && !filter.TypeIsMapped(exposedBaseType))
                    exposedBaseType = exposedBaseType.BaseType;

                if (exposedBaseType != null)
                {
                    return FromType(exposedBaseType);
                }
                return null;
            }
            return base.LoadBaseType(typeSpec);
        }


        private Type GetKnownDeclaringType(PropertyInfo propertyInfo)
        {
            // Hack, IGrouping

            var propBaseDefinition = propertyInfo.GetBaseDefinition();
            var reflectedType = propertyInfo.ReflectedType;
            return reflectedType.GetFullTypeHierarchy()
                                .Where(x => propBaseDefinition.DeclaringType.IsAssignableFrom(x))
                                .TakeUntil(x => filter.IsIndependentTypeRoot(x))
                                .LastOrDefault(x => SourceTypes.Contains(x)) ??
                   propBaseDefinition.DeclaringType;
        }

        public override TypeSpec LoadDeclaringType(PropertySpec propertySpec)
        {
            if (propertySpec is PropertyMapping)
            {
                return FromType(GetKnownDeclaringType(propertySpec.PropertyInfo));
            }
            return base.LoadDeclaringType(propertySpec);
        }


        public override PropertySpec.PropertyFlags LoadPropertyFlags(PropertySpec propertySpec)
        {
            return filter.GetPropertyFlags(propertySpec.PropertyInfo) ?? base.LoadPropertyFlags(propertySpec);
        }


        public override ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType)
        {
            var type = resourceType.Type;
            return new ResourceTypeDetails(resourceType,
                NameUtils.ConvertCamelCaseToUri(filter.GetPluralNameForType(resourceType.UriBaseType ?? resourceType)),
                this.filter.TypeIsExposedAsRepository(type),
                this.filter.GetPostReturnType(type),
                this.filter.GetParentToChildProperty(type),
                this.filter.GetChildToParentProperty(type));
        }


        public TypeSpec GetClassMapping(Type type)
        {
            type = this.filter.ResolveRealTypeForProxy(type);

            return this.mappings.GetOrCreate(type, () => CreateClassMapping(type));
        }


        public override ResourceType LoadUriBaseType(ResourceType resourceType)
        {
            Type uriBaseType = filter.GetUriBaseType(resourceType.Type);
            return uriBaseType != null ? (ResourceType)FromType(uriBaseType) : null;
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


        private TypeSpec CreateClassMapping(Type type)
        {
            return FromType(type);
        }
    }
}