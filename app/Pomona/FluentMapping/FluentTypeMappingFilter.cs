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

            var ruleMethods =
                GetMappingRulesFromObjects(fluentRuleObjects).Concat(GetMappingRulesFromDelegates(mapDelegates)).Flatten
                    (x => x.GetChildRules()).ToList();

            ApplyRules(ruleMethods);
        }


        public string ApiVersion
        {
            get { return this.wrappedFilter.ApiVersion; }
        }

        public ClientMetadata ClientMetadata
        {
            get { return this.wrappedFilter.ClientMetadata; }
        }


        public static string BuildPropertyMappingTemplate(IEnumerable<Type> types)
        {
            var typesSet = new HashSet<Type>(types);
            var sb = new StringBuilder();
            sb.Append(@"using Pomona.Example.Models;
using Pomona.FluentMapping;

namespace TestNs
{
    public class SomeFluentRules
    {
");

            foreach (var t in typesSet)
            {
                sb.AppendFormat("        public void Map(ITypeMappingConfigurator<{0}> map)\r\n        {{\r\n",
                                t.FullName);
                foreach (var p in t.GetProperties())
                {
                    if (p.DeclaringType == t || !typesSet.Contains(p.DeclaringType))
                        sb.AppendFormat("            map.Exclude(x => x.{0});\r\n", p.Name);
                }
                sb.Append("        }\r\n\r\n");
            }

            sb.Append("    }\r\n}\r\n");

            return sb.ToString();
        }


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


        public PropertyInfo GetChildToParentProperty(Type type)
        {
            return FromMappingOrDefault(type,
                                        x => x.ChildToParentProperty,
                                        () => this.wrappedFilter.GetChildToParentProperty(type));
        }


        [Obsolete("Use the ClientMetadata.AssemblyName property instead.", true)]
        public string GetClientAssemblyName()
        {
            throw new NotImplementedException("Use the ClientMetadata.AssemblyName property instead.");
        }


        public IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member)
        {
            return this.wrappedFilter.GetClientLibraryAttributes(member);
        }


        public Type GetClientLibraryType(Type type)
        {
            return this.wrappedFilter.GetClientLibraryType(type);
        }


        public LambdaExpression GetDecompiledPropertyFormula(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetDecompiledPropertyFormula(type, propertyInfo);
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


        private HttpMethod GetCombinedPropertyAccessMode(PropertyInfo propertyInfo, ConstructorSpec constructorSpec, PropertyMappingOptions opts)
        {
            var accessModeFromWrappedFilter = this.wrappedFilter.GetPropertyAccessMode(propertyInfo, constructorSpec);
            accessModeFromWrappedFilter |= PatchOfTypeIsAllowed(propertyInfo.PropertyType) ? HttpMethod.Patch : 0;

            Type elementType;
            if (propertyInfo.PropertyType.TryGetEnumerableElementType(out elementType))
            {
                accessModeFromWrappedFilter |= 
                    (TypeIsMappedAsTransformedType(elementType) && PostOfTypeIsAllowed(elementType))
                    ? HttpMethod.Post : 0;
            }
            return (opts.Method & opts.MethodMask) | (accessModeFromWrappedFilter & ~(opts.MethodMask));
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


        public Func<object, IContainer, object> GetPropertyGetter(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.OnGetDelegate,
                                        () => this.wrappedFilter.GetPropertyGetter(type, propertyInfo));
        }


        public HttpMethod GetPropertyItemAccessMode(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x =>
                                            GetCombinedPropertyItemAccessMode(type, propertyInfo, x),
                                        () => this.wrappedFilter.GetPropertyItemAccessMode(type, propertyInfo));
        }


        private HttpMethod GetCombinedPropertyItemAccessMode(Type type, PropertyInfo propertyInfo, PropertyMappingOptions opts)
        {
            var accessModeFromWrappedFilter = this.wrappedFilter.GetPropertyItemAccessMode(type, propertyInfo);
            accessModeFromWrappedFilter |= this.wrappedFilter.PatchOfTypeIsAllowed(type) ? HttpMethod.Patch : 0;
            return (opts.ItemMethod & opts.ItemMethodMask)
                   | (accessModeFromWrappedFilter
                      & ~(opts.ItemMethodMask));
        }


        public string GetPropertyMappedName(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.Name,
                                        () => this.wrappedFilter.GetPropertyMappedName(type, propertyInfo));
        }


        public Action<object, object, IContainer> GetPropertySetter(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.OnSetDelegate,
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


        public bool PropertyFormulaIsDecompiled(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.PropertyFormulaIsDecompiled,
                                        () => this.wrappedFilter.PropertyFormulaIsDecompiled(type, propertyInfo));
        }


        public bool PropertyIsAlwaysExpanded(Type type, PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(type,
                                        propertyInfo,
                                        x => x.AlwaysExpanded,
                                        () => this.wrappedFilter.PropertyIsAlwaysExpanded(type, propertyInfo));
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
                        string.Format(
                            "All properties are required to be explicitly included or excluded from mapping, but {0} of {1} is neither.",
                            propertyInfo.Name,
                            propertyInfo.DeclaringType.FullName));
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


        private static IEnumerable<RuleMethod> GetMappingRulesFromDelegates(IEnumerable<Delegate> mapDelegates)
        {
            return mapDelegates == null
                ? Enumerable.Empty<RuleMethod>()
                : mapDelegates.Where(x => IsRuleMethod(x.Method)).Select(x => new RuleMethod(x.Method, x.Target));
        }


        private static IEnumerable<RuleMethod> GetMappingRulesFromObjects(IEnumerable<object> ruleContainers)
        {
            if (ruleContainers == null)
                return Enumerable.Empty<RuleMethod>();
            return ruleContainers
                .SelectMany(x => x.GetType()
                                .GetMethods()
                                .Where(IsRuleMethod)
                                .Select(m => new RuleMethod(m, x)));
        }


        private static bool IsRuleMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;
            var paramType = parameters[0].ParameterType;

            // Metadata token is the same across all generic type instances and generic type definition
            return paramType.UniqueToken() == typeof(ITypeMappingConfigurator<>).UniqueToken();
        }


        private void ApplyRules(IEnumerable<RuleMethod> ruleMethods)
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


        [Obsolete("Use the ClientMetadata.InformationalVersion property instead.", true)]
        string ITypeMappingFilter.GetClientInformationalVersion()
        {
            return this.wrappedFilter.GetClientInformationalVersion();
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

        #region Nested type: NestedTypeMappingConfigurator

        private class NestedTypeMappingConfigurator<TDeclaring> : TypeMappingConfiguratorBase<TDeclaring>
        {
            private readonly List<Delegate> typeConfigurationDelegates = new List<Delegate>();


            public NestedTypeMappingConfigurator(List<Delegate> typeConfigurationDelegates)
            {
                this.typeConfigurationDelegates = typeConfigurationDelegates;
            }


            protected override ITypeMappingConfigurator<TDeclaring> OnHasChild<TItem>(
                Expression<Func<TDeclaring, TItem>> childProperty,
                Expression<Func<TItem, TDeclaring>> parentProperty,
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
                Func<IPropertyOptionsBuilder<TDeclaring, TItem>, IPropertyOptionsBuilder<TDeclaring, TItem>>
                    propertyOptions)
            {
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping =
                    x => x.AsChildResourceOf(parentProperty, childProperty);
                this.typeConfigurationDelegates.Add(asChildResourceMapping);
                this.typeConfigurationDelegates.Add(typeOptions);
                return this;
            }


            protected override ITypeMappingConfigurator<TDeclaring> OnHasChildren<TItem>(
                Expression<Func<TDeclaring, IEnumerable<TItem>>> collectionProperty,
                Expression<Func<TItem, TDeclaring>> parentProperty,
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
                Func
                    <IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>,
                    IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
            {
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping =
                    x => x.AsChildResourceOf(parentProperty, collectionProperty);
                this.typeConfigurationDelegates.Add(asChildResourceMapping);
                this.typeConfigurationDelegates.Add(typeOptions);
                return this;
            }
        }

        #endregion

        #region Nested type: RuleMethod

        private class RuleMethod
        {
            public static readonly MethodInfo getChildRulesMethod =
                ReflectionHelper.GetMethodDefinition<RuleMethod>(x => x.GetChildRules<object>());

            private readonly Type appliesToType;

            private readonly object instance;
            private readonly MethodInfo method;


            public RuleMethod(MethodInfo method, object instance)
            {
                this.appliesToType = method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
                this.method = method;
                this.instance = instance;
            }


            public override string ToString()
            {
                var declaringType = method.DeclaringType;
                return string.Format("{1}.{2} for {0}", appliesToType.Name, declaringType != null ? declaringType.Name : "?", method.Name);
            }


            public Type AppliesToType
            {
                get { return this.appliesToType; }
            }

            public object Instance
            {
                get { return this.instance; }
            }

            public MethodInfo Method
            {
                get { return this.method; }
            }


            public IEnumerable<RuleMethod> GetChildRules()
            {
                return (IEnumerable<RuleMethod>)getChildRulesMethod.MakeGenericMethod(AppliesToType).Invoke(this, null);
            }


            private IEnumerable<RuleMethod> GetChildRules<T>()
            {
                var typeConfigDelegates = new List<Delegate>();
                var nestedScanner = new NestedTypeMappingConfigurator<T>(typeConfigDelegates);
                Method.Invoke(Instance, new object[] { nestedScanner });
                return typeConfigDelegates.Select(x => new RuleMethod(x.Method, x.Target)).ToList();
            }
        }

        #endregion
    }
}