#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.TypeSystem;

namespace Pomona.FluentMapping
{
    public sealed class FluentTypeMappingFilter : ITypeMappingFilter
    {
        private readonly IEnumerable<Type> sourceTypes;

        private readonly ConcurrentDictionary<string, TypeMappingOptions> typeMappingDict =
            new ConcurrentDictionary<string, TypeMappingOptions>();

        private readonly ITypeMappingFilter wrappedFilter;


        public FluentTypeMappingFilter(ITypeMappingFilter wrappedFilter,
                                       IEnumerable<object> fluentRuleObjects,
                                       IEnumerable<Delegate> mapDelegates,
                                       IEnumerable<Type> sourceTypes)
        {
            this.wrappedFilter = wrappedFilter;
            this.sourceTypes = sourceTypes ?? Enumerable.Empty<Type>();

            var ruleMethods = FluentRuleMethodScanner.Scan(fluentRuleObjects, mapDelegates);

            ApplyRules(ruleMethods);
        }



        internal TypeMappingOptions GetTypeMapping(Type type)
        {
            return this.typeMappingDict.GetOrAdd(type.FullName, k =>
            {
                TypeMappingOptions typeMapping = new TypeMappingOptions(type);
                typeMapping.DefaultPropertyInclusionMode =
                    GetDefaultPropertyInclusionMode();
                return typeMapping;
            });
        }


        private void ApplyRules(IEnumerable<FluentRuleMethod> ruleMethods)
        {
            var allTransformedTypes = this.sourceTypes.Where(TypeIsMappedAsTransformedType).ToList();

            // NOTE: We need to order the properties in ascending order by how
            //       specific their declaring types are so we get the most
            //       specific ones last.
            ruleMethods = ruleMethods.OrderBy(x => x.AppliesToType, new SubclassComparer());

            foreach (var ruleMethod in ruleMethods)
            {
                var appliesToType = ruleMethod.AppliesToType;
                foreach (var subType in allTransformedTypes.Where(appliesToType.IsAssignableFrom))
                {
                    var typeMapping = GetTypeMapping(subType);
                    var configurator = typeMapping.GetConfigurator(ruleMethod.AppliesToType);
                    ruleMethod.Method.Invoke(ruleMethod.Instance, new[] { configurator });
                }
            }
        }


        private T FromMappingOrDefault<T>(Type type,
                                          Func<TypeMappingOptions, T?> ifMappingExist,
                                          Func<T> ifMappingMissing)
            where T : struct
        {
            var result = FromMappingOrDefault(type, ifMappingExist, () => (T?)ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(Type type,
                                          Func<TypeMappingOptions, T> ifMappingExist,
                                          Func<T> ifMappingMissing)
        {
            TypeMappingOptions typeMappingOptions;
            object result = null;
            if (this.typeMappingDict.TryGetValue(type.FullName, out typeMappingOptions))
                result = ifMappingExist(typeMappingOptions);
            if (result == null)
                return ifMappingMissing();
            return (T)result;
        }


        private T FromMappingOrDefault<T>(Type type,
                                          PropertyInfo propertyInfo,
                                          Func<PropertyMappingOptions, T> ifMappingExist,
                                          Func<T> ifMappingMissing)
        {
            TypeMappingOptions typeMappingOptions;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(type, propertyInfo, out typeMappingOptions, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T)result;
        }


        private T FromMappingOrDefault<T>(Type type,
                                          PropertyInfo propertyInfo,
                                          Func<PropertyMappingOptions, T?> ifMappingExist,
                                          Func<T> ifMappingMissing)
            where T : struct
        {
            TypeMappingOptions typeMappingOptions;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(type, propertyInfo, out typeMappingOptions, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T)result;
        }


        private HttpMethod GetCombinedPropertyAccessMode(PropertyInfo propertyInfo,
                                                         ConstructorSpec constructorSpec,
                                                         PropertyMappingOptions opts)
        {
            var accessModeFromWrappedFilter = this.wrappedFilter.GetPropertyAccessMode(propertyInfo, constructorSpec);
            accessModeFromWrappedFilter |= PatchOfTypeIsAllowed(propertyInfo.PropertyType) ? HttpMethod.Patch : 0;

            Type elementType;
            if (propertyInfo.PropertyType.TryGetEnumerableElementType(out elementType))
            {
                accessModeFromWrappedFilter |=
                    (TypeIsMappedAsTransformedType(elementType) && PostOfTypeIsAllowed(elementType))
                        ? HttpMethod.Post
                        : 0;
            }
            return (opts.Method & opts.MethodMask) | (accessModeFromWrappedFilter & ~(opts.MethodMask));
        }


        private HttpMethod GetCombinedPropertyItemAccessMode(Type type, PropertyInfo propertyInfo, PropertyMappingOptions opts)
        {
            var accessModeFromWrappedFilter = this.wrappedFilter.GetPropertyItemAccessMode(type, propertyInfo);
            accessModeFromWrappedFilter |= this.wrappedFilter.PatchOfTypeIsAllowed(type) ? HttpMethod.Patch : 0;
            return (opts.ItemMethod & opts.ItemMethodMask)
                   | (accessModeFromWrappedFilter
                      & ~(opts.ItemMethodMask));
        }


        private bool TryGetTypeMappingAndPropertyOptions(Type reflectedType,
                                                         PropertyInfo propertyInfo,
                                                         out TypeMappingOptions typeMapping,
                                                         out PropertyMappingOptions propertyOptions)
        {
            typeMapping = GetTypeMapping(reflectedType);
            propertyOptions = typeMapping.GetPropertyOptions(propertyInfo);
            return true;
        }


        public string ApiVersion => this.wrappedFilter.ApiVersion;


        public bool ClientEnumIsGeneratedAsStringEnum(Type enumType)
        {
            return this.wrappedFilter.ClientEnumIsGeneratedAsStringEnum(enumType);
        }


        public ClientMetadata ClientMetadata => this.wrappedFilter.ClientMetadata;


        public bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo.ReflectedType, propertyInfo,
                                        x => x.ExposedAsRepository,
                                        () => this.wrappedFilter.ClientPropertyIsExposedAsRepository(propertyInfo));
        }


        public bool DeleteOfTypeIsAllowed(Type type)
        {
            return FromMappingOrDefault(type, x => x.DeleteAllowed, () => this.wrappedFilter.DeleteOfTypeIsAllowed(type));
        }


        public bool GenerateIndependentClient()
        {
            return this.wrappedFilter.GenerateIndependentClient();
        }


        public IEnumerable<PropertyInfo> GetAllPropertiesOfType(Type type, BindingFlags bindingFlags)
        {
            TypeMappingOptions typeMappingOptions;
            var properties = this.wrappedFilter.GetAllPropertiesOfType(type, bindingFlags);
            if (this.typeMappingDict.TryGetValue(type.FullName, out typeMappingOptions))
                return properties.Concat(typeMappingOptions.VirtualProperties);

            return properties;
        }


        public PropertyInfo GetChildToParentProperty(Type type)
        {
            return FromMappingOrDefault(type,
                                        x => x.ChildToParentProperty,
                                        () => this.wrappedFilter.GetChildToParentProperty(type));
        }


        public IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member)
        {
            return this.wrappedFilter.GetClientLibraryAttributes(member);
        }


        public Type GetClientLibraryType(Type type)
        {
            return this.wrappedFilter.GetClientLibraryType(type);
        }


        public DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return this.wrappedFilter.GetDefaultPropertyInclusionMode();
        }


        public JsonConverter GetJsonConverterForType(Type type)
        {
            return this.wrappedFilter.GetJsonConverterForType(type);
        }


        public Action<object> GetOnDeserializedHook(Type type)
        {
            return FromMappingOrDefault(type,
                                        x => x.OnDeserialized,
                                        () => this.wrappedFilter.GetOnDeserializedHook(type));
        }


        public PropertyInfo GetParentToChildProperty(Type type)
        {
            return FromMappingOrDefault(type,
                                        x => x.ParentToChildProperty,
                                        () => this.wrappedFilter.GetParentToChildProperty(type));
        }


        public string GetPluralNameForType(Type type)
        {
            return FromMappingOrDefault(type, x => x.PluralName, () => this.wrappedFilter.GetPluralNameForType(type));
        }


        public Type GetPostReturnType(Type type)
        {
            return FromMappingOrDefault(
                type,
                x => x.PostResponseType ?? this.wrappedFilter.GetPostReturnType(type),
                () => this.wrappedFilter.GetPostReturnType(type));
        }


        public HttpMethod GetPropertyAccessMode(PropertyInfo propertyInfo, ConstructorSpec constructorSpec)
        {
            return FromMappingOrDefault(
                propertyInfo.ReflectedType,
                propertyInfo,
                x =>
                    GetCombinedPropertyAccessMode(propertyInfo, constructorSpec, x),
                () => this.wrappedFilter.GetPropertyAccessMode(propertyInfo, constructorSpec));
        }


        public IEnumerable<Attribute> GetPropertyAttributes(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        o =>
                                            this.wrappedFilter.GetPropertyAttributes(type, propertyInfo).EmptyIfNull()
                                                .Concat(o.AddedAttributes),
                                        () => this.wrappedFilter.GetPropertyAttributes(type, propertyInfo));
        }


        public PropertyCreateMode GetPropertyCreateMode(Type type,
                                                        PropertyInfo propertyInfo,
                                                        ParameterInfo ctorParameterInfo)
        {
            return FromMappingOrDefault(type, propertyInfo,
                                        x => x.CreateMode,
                                        () =>
                                            this.wrappedFilter.GetPropertyCreateMode(type, propertyInfo,
                                                                                     ctorParameterInfo));
        }


        public ExpandMode GetPropertyExpandMode(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        o => o.PropertyExpandMode,
                                        () => this.wrappedFilter.GetPropertyExpandMode(type, propertyInfo));
        }


        public PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyFlags(propertyInfo);
        }


        public LambdaExpression GetPropertyFormula(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type, propertyInfo,
                                        x => x.Formula,
                                        () => this.wrappedFilter.GetPropertyFormula(type, propertyInfo));
        }


        public PropertyGetter GetPropertyGetter(Type type, PropertyInfo propertyInfo)
        {
            var getter = FromMappingOrDefault(type,
                                              propertyInfo,
                                              x => (PropertyGetter)x.OnGetDelegate,
                                              () => this.wrappedFilter.GetPropertyGetter(type, propertyInfo));
            return getter;
        }


        public HttpMethod GetPropertyItemAccessMode(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x =>
                                            GetCombinedPropertyItemAccessMode(type, propertyInfo, x),
                                        () => this.wrappedFilter.GetPropertyItemAccessMode(type, propertyInfo));
        }


        public string GetPropertyMappedName(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.Name,
                                        () => this.wrappedFilter.GetPropertyMappedName(type, propertyInfo));
        }


        public PropertySetter GetPropertySetter(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => (PropertySetter)x.OnSetDelegate,
                                        () => this.wrappedFilter.GetPropertySetter(type, propertyInfo));
        }


        public Type GetPropertyType(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        o => o.PropertyType,
                                        () => this.wrappedFilter.GetPropertyType(type, propertyInfo));
        }


        public IEnumerable<Type> GetResourceHandlers(Type type)
        {
            return FromMappingOrDefault(type, x => x.HandlerTypes, () => this.wrappedFilter.GetResourceHandlers(type).EmptyIfNull());
        }


        public ConstructorSpec GetTypeConstructor(Type type)
        {
            return FromMappingOrDefault(type, x => x.Constructor, () => this.wrappedFilter.GetTypeConstructor(type));
        }


        public bool GetTypeIsAbstract(Type type)
        {
            return FromMappingOrDefault(type, x => x.IsAbstract, () => this.wrappedFilter.GetTypeIsAbstract(type));
        }


        public string GetTypeMappedName(Type type)
        {
            return FromMappingOrDefault(type, x => x.Name, () => this.wrappedFilter.GetTypeMappedName(type));
        }


        public Type GetUriBaseType(Type type)
        {
            // TODO: Support this convention, not completely sure how it will work :/ [KNS]
            return this.wrappedFilter.GetUriBaseType(type);
        }


        public string GetUrlRelativePath(Type type)
        {
            return FromMappingOrDefault(type, x => x.UrlRelativePath, () => this.wrappedFilter.GetUrlRelativePath(type));
        }


        public bool IsIndependentTypeRoot(Type type)
        {
            return FromMappingOrDefault(type,
                                        tmo => tmo.IsIndependentTypeRoot,
                                        () => this.wrappedFilter.IsIndependentTypeRoot(type));
        }


        public bool PatchOfTypeIsAllowed(Type type)
        {
            return FromMappingOrDefault(type, x => x.PatchAllowed, () => this.wrappedFilter.PatchOfTypeIsAllowed(type));
        }


        public bool PostOfTypeIsAllowed(Type type)
        {
            return FromMappingOrDefault(type, x => x.PostAllowed, () => this.wrappedFilter.PostOfTypeIsAllowed(type));
        }


        public bool PropertyIsAttributes(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.IsAttributesProperty,
                                        () => this.wrappedFilter.PropertyIsAttributes(type, propertyInfo));
        }


        public bool PropertyIsEtag(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.IsEtagProperty,
                                        () => this.wrappedFilter.PropertyIsEtag(type, propertyInfo));
        }


        public bool PropertyIsIncluded(Type type, PropertyInfo propertyInfo)
        {
            TypeMappingOptions typeMapping;
            PropertyMappingOptions propertyOptions;
            if (!TryGetTypeMappingAndPropertyOptions(type, propertyInfo, out typeMapping, out propertyOptions))
                return this.wrappedFilter.PropertyIsIncluded(type, propertyInfo);

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Excluded)
                return false;

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Included)
                return true;

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Default)
            {
                if (typeMapping.DefaultPropertyInclusionMode ==
                    DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault)
                    return this.wrappedFilter.PropertyIsIncluded(type, propertyInfo);

                if (typeMapping.DefaultPropertyInclusionMode ==
                    DefaultPropertyInclusionMode.AllPropertiesRequiresExplicitMapping)
                {
                    throw new PomonaMappingException(
                        $"All properties are required to be explicitly included or excluded from mapping, but {propertyInfo.Name} of {propertyInfo.DeclaringType.FullName} is neither.");
                }
            }

            return false;
        }


        public bool PropertyIsPrimaryId(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.IsPrimaryKey,
                                        () => this.wrappedFilter.PropertyIsPrimaryId(type, propertyInfo));
        }


        public Type ResolveRealTypeForProxy(Type type)
        {
            return this.wrappedFilter.ResolveRealTypeForProxy(type);
        }


        public bool TypeIsExposedAsRepository(Type type)
        {
            return FromMappingOrDefault(type,
                                        x => x.IsExposedAsRepository,
                                        () => this.wrappedFilter.TypeIsExposedAsRepository(type));
        }


        public bool TypeIsMapped(Type type)
        {
            return this.wrappedFilter.TypeIsMapped(type);
        }


        public bool TypeIsMappedAsCollection(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsCollection(type);
        }


        public bool TypeIsMappedAsSharedType(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsSharedType(type);
        }


        public bool TypeIsMappedAsTransformedType(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsTransformedType(type);
        }


        public bool TypeIsMappedAsValueObject(Type type)
        {
            return FromMappingOrDefault(type,
                                        x => x.IsValueObject,
                                        () => this.wrappedFilter.TypeIsMappedAsValueObject(type));
        }


        public bool TypeIsSingletonResource(Type type)
        {
            return FromMappingOrDefault(type, x => x.IsSingleton, () => this.wrappedFilter.TypeIsSingletonResource(type));
        }

        #region Nested type: NestedTypeMappingConfigurator

        #endregion

        #region Nested type: RuleMethod

        #endregion
    }
}
