#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
                throw new ArgumentNullException(nameof(filter));
            if (sourceTypes == null)
                throw new ArgumentNullException(nameof(sourceTypes));

            Filter = filter;

            this.sourceTypes = new HashSet<Type>(sourceTypes.Where(Filter.TypeIsMapped));

            this.typeNameMap = this.sourceTypes.Select(FromType).ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            if (onMappingComplete != null)
                onMappingComplete(this);
        }


        public IEnumerable<EnumTypeSpec> EnumTypes
        {
            get { return TypeMap.Values.OfType<EnumTypeSpec>(); }
        }

        public ITypeMappingFilter Filter { get; }

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


        public override TypeSpec LoadBaseType(TypeSpec typeSpec)
        {
            if (typeSpec is StructuredType)
            {
                if (Filter.IsIndependentTypeRoot(typeSpec))
                    return null;

                var exposedBaseType = typeSpec.Type.BaseType;

                while (exposedBaseType != null && !Filter.TypeIsMapped(exposedBaseType))
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
                return Filter.GetTypeConstructor(typeSpec);
            return base.LoadConstructor(typeSpec);
        }


        public override IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec)
        {
            var attrs = base.LoadDeclaredAttributes(memberSpec);

            var typeSpec = memberSpec as TypeSpec;
            if (typeSpec != null)
            {
                var customClientLibraryType = Filter.GetClientLibraryType(typeSpec.Type);
                if (customClientLibraryType != null)
                    attrs = attrs.Append(new CustomClientLibraryTypeAttribute(customClientLibraryType));
                var customJsonConverter = Filter.GetJsonConverterForType(typeSpec.Type);
                if (customJsonConverter != null)
                    attrs = attrs.Append(new CustomJsonConverterAttribute(customJsonConverter));
            }
            var propSpec = memberSpec as PropertySpec;
            if (propSpec != null)
            {
                var formulaExpr = Filter.GetPropertyFormula(propSpec.ReflectedType, propSpec.PropertyInfo);
                if (formulaExpr != null)
                    attrs = attrs.Append(new PropertyFormulaAttribute(formulaExpr));

                attrs =
                    attrs.Concat(
                        Filter.GetPropertyAttributes(propSpec.ReflectedType, propSpec.PropertyInfo).EmptyIfNull());
            }
            return attrs;
        }


        public override TypeSpec LoadDeclaringType(PropertySpec propertySpec)
        {
            if (propertySpec is StructuredProperty)
                return FromType(GetKnownDeclaringType(propertySpec.ReflectedType, propertySpec.PropertyInfo));
            return base.LoadDeclaringType(propertySpec);
        }


        public override PropertyGetter LoadGetter(PropertySpec propertySpec)
        {
            return Filter.GetPropertyGetter(propertySpec.ReflectedType, propertySpec.PropertyInfo)
                   ?? base.LoadGetter(propertySpec);
        }


        public override IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec)
        {
            if (typeSpec is StructuredType)
                return base.LoadInterfaces(typeSpec).Where(x => Filter.TypeIsMappedAsTransformedType(x));

            return base.LoadInterfaces(typeSpec);
        }


        public override string LoadName(MemberSpec memberSpec)
        {
            return memberSpec
                .Maybe()
                .Switch()
                .Case<PropertySpec>().Then(x => Filter.GetPropertyMappedName(x.ReflectedType, x.PropertyInfo))
                .Case<TypeSpec>().Then(x => Filter.GetTypeMappedName(x.Type))
                .EndSwitch()
                .OrDefault(() => base.LoadName(memberSpec));
        }


        public override IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec)
        {
            if (typeSpec is ComplexType || typeSpec is ResourceType)
            {
                var propertiesFromNonMappedInterfaces = typeSpec.Type.IsInterface
                    ? typeSpec.Type.GetInterfaces().Where(x => !Filter.TypeIsMapped(x)).SelectMany(
                        x => Filter.GetAllPropertiesOfType(x, BindingFlags.Instance | BindingFlags.Public))
                    : Enumerable.Empty<PropertyInfo>();

                return Filter.GetAllPropertiesOfType(typeSpec,
                                                     BindingFlags.Instance | BindingFlags.Static
                                                     | BindingFlags.Public
                                                     | BindingFlags.NonPublic)
                             .Concat(propertiesFromNonMappedInterfaces)
                             .Where(x => Filter.PropertyIsIncluded(typeSpec.Type, x))
                             .Select(x => WrapProperty(typeSpec, x));
            }

            return base.LoadProperties(typeSpec);
        }


        public override PropertyFlags LoadPropertyFlags(PropertySpec propertySpec)
        {
            return Filter.GetPropertyFlags(propertySpec.PropertyInfo) ?? base.LoadPropertyFlags(propertySpec);
        }


        public override TypeSpec LoadPropertyType(PropertySpec propertySpec)
        {
            var complexProperty = propertySpec as StructuredProperty;
            if (complexProperty != null)
                return FromType(Filter.GetPropertyType(complexProperty.ReflectedType, complexProperty.PropertyInfo));
            return base.LoadPropertyType(propertySpec);
        }


        public override ResourcePropertyDetails LoadResourcePropertyDetails(ResourceProperty property)
        {
            var propInfo = property.PropertyInfo;
            return new ResourcePropertyDetails(Filter.ClientPropertyIsExposedAsRepository(propInfo),
                                               NameUtils.ConvertCamelCaseToUri(Filter.GetPropertyMappedName(property.ReflectedType,
                                                                                                            propInfo)));
        }


        public override ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType)
        {
            var type = resourceType.Type;
            var parentToChildProperty = Filter.GetParentToChildProperty(type);
            var childToParentProperty = Filter.GetChildToParentProperty(type);
            var isRootResource = parentToChildProperty == null;

            var relativeResourcePath = isRootResource ? Filter.GetUrlRelativePath(type).TrimStart('/') : null;

            return new ResourceTypeDetails(resourceType,
                                           relativeResourcePath,
                                           Filter.TypeIsExposedAsRepository(type),
                                           Filter.GetPostReturnType(type),
                                           parentToChildProperty,
                                           childToParentProperty,
                                           Filter.TypeIsSingletonResource(type),
                                           Filter.GetResourceHandlers(type),
                                           Filter.GetPluralNameForType(type));
        }


        public override PropertySetter LoadSetter(PropertySpec propertySpec)
        {
            return Filter.GetPropertySetter(propertySpec.ReflectedType, propertySpec.PropertyInfo)
                   ?? base.LoadSetter(propertySpec);
        }


        public override StructuredPropertyDetails LoadStructuredPropertyDetails(StructuredProperty property)
        {
            var propInfo = property.PropertyInfo;

            var reflectedType = property.ReflectedType;
            var expandMode = Filter.GetPropertyExpandMode(reflectedType, propInfo);
            var accessMode = Filter.GetPropertyAccessMode(propInfo, property.DeclaringType.Constructor);

            var details = new StructuredPropertyDetails(
                Filter.PropertyIsAttributes(reflectedType, propInfo),
                Filter.PropertyIsEtag(reflectedType, propInfo),
                Filter.PropertyIsPrimaryId(reflectedType, propInfo),
                accessMode.HasFlag(HttpMethod.Get),
                accessMode,
                Filter.GetPropertyItemAccessMode(reflectedType, propInfo),
                expandMode);
            return details;
        }


        public override StructuredTypeDetails LoadStructuredTypeDetails(StructuredType structuredType)
        {
            // TODO: Get allowed methods from filter
            var allowedMethods = HttpMethod.Get |
                                 (Filter.PatchOfTypeIsAllowed(structuredType) ? HttpMethod.Patch : 0) |
                                 (Filter.PostOfTypeIsAllowed(structuredType) ? HttpMethod.Post : 0) |
                                 (Filter.DeleteOfTypeIsAllowed(structuredType) ? HttpMethod.Delete : 0);

            var type = structuredType.Type;
            var details = new StructuredTypeDetails(structuredType,
                                                    allowedMethods,
                                                    Filter.GetOnDeserializedHook(type),
                                                    Filter.TypeIsMappedAsValueObject(type),
                                                    Filter.TypeIsMappedAsValueObject(type),
                                                    Filter.GetTypeIsAbstract(type));

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
            Type uriBaseType = Filter.GetUriBaseType(resourceType.Type);
            return uriBaseType != null ? (ResourceType)FromType(uriBaseType) : null;
        }


        public override bool TryGetTypeByName(string typeName, out TypeSpec typeSpec)
        {
            return base.TryGetTypeByName(typeName, out typeSpec) || this.typeNameMap.TryGetValue(typeName, out typeSpec);
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
            // TODO: This double negation makes my head hurt. Un-negate pl0x. @asbjornu
            var typeIsNotMappedAsSharedTypeAndIsMappedAsTransformedType =
                !Filter.TypeIsMappedAsSharedType(type) && Filter.TypeIsMappedAsTransformedType(type);

            if (!typeIsNotMappedAsSharedTypeAndIsMappedAsTransformedType)
                return base.CreateType(type);

            if (Filter.TypeIsMappedAsValueObject(type))
                return new ComplexType(this, type);

            return new ResourceType(this, type);
        }


        protected override sealed Type MapExposedClrType(Type type)
        {
            return Filter.ResolveRealTypeForProxy(type);
        }


        private Type GetKnownDeclaringType(Type reflectedType, PropertyInfo propertyInfo)
        {
            var propBaseDefinition = propertyInfo.GetBaseDefinition();
            return reflectedType.GetFullTypeHierarchy()
                                .Where(x => propBaseDefinition.DeclaringType != null
                                            && propBaseDefinition.DeclaringType.IsAssignableFrom(x))
                                .TakeUntil(x => Filter.IsIndependentTypeRoot(x))
                                .LastOrDefault(x => this.sourceTypes.Contains(x))
                   ?? propBaseDefinition.DeclaringType;
        }
    }
}