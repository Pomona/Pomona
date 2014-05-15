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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.TypeSystem;

namespace Pomona.FluentMapping
{
    public sealed class FluentTypeMappingFilter : ITypeMappingFilter
    {
        private class NestedTypeMappingConfigurator<TDeclaring> : TypeMappingConfiguratorBase<TDeclaring>
        {
            readonly List<Delegate> typeConfigurationDelegates = new List<Delegate>();


            public NestedTypeMappingConfigurator(List<Delegate> typeConfigurationDelegates)
            {
                this.typeConfigurationDelegates = typeConfigurationDelegates;
            }


            public override ITypeMappingConfigurator<TDeclaring> HasChildren<TItem>(Expression<Func<TDeclaring, IEnumerable<TItem>>> collectionProperty, Expression<Func<TItem, TDeclaring>> parentProperty, Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions, Func<IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>, IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
            {
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping = x => x.AsChildResourceOf(parentProperty, collectionProperty);
                typeConfigurationDelegates.Add(asChildResourceMapping);
                typeConfigurationDelegates.Add(typeOptions);
                return this;
            }
        }

        private readonly ConcurrentDictionary<string, TypeMappingOptions> typeMappingDict =
            new ConcurrentDictionary<string, TypeMappingOptions>();

        private readonly ITypeMappingFilter wrappedFilter;
        private readonly IEnumerable<Type> sourceTypes;


        public FluentTypeMappingFilter(ITypeMappingFilter wrappedFilter, IEnumerable<object> fluentRuleObjects, IEnumerable<Delegate> mapDelegates, IEnumerable<Type> sourceTypes)
        {
            this.wrappedFilter = wrappedFilter;
            this.sourceTypes = sourceTypes ?? Enumerable.Empty<Type>();

            var ruleMethods =
                GetMappingRulesFromObjects(fluentRuleObjects).Concat(GetMappingRulesFromDelegates(mapDelegates)).Flatten
                    (x => x.GetChildRules());

            ApplyRules(ruleMethods);
        }

        private IEnumerable<RuleMethod> GetMappingRulesFromDelegates(IEnumerable<Delegate> mapDelegates)
        {
            if (mapDelegates == null)
                return Enumerable.Empty<RuleMethod>();

            return mapDelegates.Where(x => IsRuleMethod(x.Method)).Select(x => new RuleMethod(x.Method, x.Target));
        }


        public string ApiVersion
        {
            get { return this.wrappedFilter.ApiVersion; }
        }


        public static string BuildPropertyMappingTemplate(IEnumerable<Type> types)
        {
            var typesSet = new HashSet<Type>(types);
            var sb = new StringBuilder();
            sb.Append(
                @"using Pomona.Example.Models;
using Pomona.FluentMapping;

namespace TestNs
{
    public class SomeFluentRules
    {
");

            foreach (var t in typesSet)
            {
                sb.AppendFormat(
                    "        public void Map(ITypeMappingConfigurator<{0}> map)\r\n        {{\r\n",
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
            return FromMappingOrDefault(propertyInfo,
                x => x.ExposedAsRepository,
                () => wrappedFilter.ClientPropertyIsExposedAsRepository(propertyInfo));
        }


        public string GetClientAssemblyName()
        {
            return this.wrappedFilter.GetClientAssemblyName();
        }


        string ITypeMappingFilter.GetClientInformationalVersion()
        {
            return this.wrappedFilter.GetClientInformationalVersion();
        }


        public Type GetClientLibraryType(Type type)
        {
            return this.wrappedFilter.GetClientLibraryType(type);
        }


        public IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member)
        {
            return this.wrappedFilter.GetClientLibraryAttributes(member);
        }


        public bool GenerateIndependentClient()
        {
            return this.wrappedFilter.GenerateIndependentClient();
        }


        public LambdaExpression GetDecompiledPropertyFormula(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetDecompiledPropertyFormula(propertyInfo);
        }


        public DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return this.wrappedFilter.GetDefaultPropertyInclusionMode();
        }

        public JsonConverter GetJsonConverterForType(Type type)
        {
            return this.wrappedFilter.GetJsonConverterForType(type);
        }


        public bool DeleteOfTypeIsAllowed(Type type)
        {
            return FromMappingOrDefault(type, x => x.DeleteAllowed, () => wrappedFilter.DeleteOfTypeIsAllowed(type));
        }


        public Action<object> GetOnDeserializedHook(Type type)
        {
            return FromMappingOrDefault(type,
                x => x.OnDeserialized,
                () => this.wrappedFilter.GetOnDeserializedHook(type));
        }


        public HttpMethod GetPropertyItemAccessMode(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x =>
                    (x.ItemMethod & x.ItemMethodMask)
                    | (this.wrappedFilter.GetPropertyItemAccessMode(propertyInfo) & ~(x.ItemMethodMask)),
                () => this.wrappedFilter.GetPropertyItemAccessMode(propertyInfo));
        }


        public PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
        {
            return wrappedFilter.GetPropertyFlags(propertyInfo);
        }


        public IEnumerable<Type> GetResourceHandlers(Type type)
        {
            return FromMappingOrDefault(type, x => x.HandlerTypes, () => wrappedFilter.GetResourceHandlers(type));
        }

        public bool GetTypeIsAbstract(Type type)
        {
            return this.wrappedFilter.GetTypeIsAbstract(type);
        }


        public PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty)
        {
            return this.wrappedFilter.GetOneToManyCollectionForeignKey(collectionProperty);
        }


        public string GetTypeMappedName(Type type)
        {
            return FromMappingOrDefault(type, x => x.Name, () => wrappedFilter.GetTypeMappedName(type));
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
            return FromMappingOrDefault(propertyInfo,
                x =>
                    (x.Method & x.MethodMask)
                    | (this.wrappedFilter.GetPropertyAccessMode(propertyInfo, constructorSpec) & ~(x.MethodMask)),
                () => this.wrappedFilter.GetPropertyAccessMode(propertyInfo, constructorSpec));
        }


        public PropertyCreateMode GetPropertyCreateMode(PropertyInfo propertyInfo, ParameterInfo ctorParameterInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.CreateMode,
                () => this.wrappedFilter.GetPropertyCreateMode(propertyInfo, ctorParameterInfo));
        }


        public LambdaExpression GetPropertyFormula(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.Formula,
                () => this.wrappedFilter.GetPropertyFormula(propertyInfo));
        }


        public Func<object, IContextResolver, object> GetPropertyGetter(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.OnGetDelegate,
                () => this.wrappedFilter.GetPropertyGetter(propertyInfo));
        }


        public string GetPropertyMappedName(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(
                propertyInfo,
                x => x.Name,
                () => this.wrappedFilter.GetPropertyMappedName(propertyInfo));
        }


        public Action<object, object, IContextResolver> GetPropertySetter(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.OnSetDelegate,
                () => wrappedFilter.GetPropertySetter(propertyInfo));
        }


        public Type GetPropertyType(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyType(propertyInfo);
        }

        public ConstructorSpec GetTypeConstructor(Type type)
        {
            return FromMappingOrDefault(type, x => x.Constructor, () => this.wrappedFilter.GetTypeConstructor(type));
        }


        public Type GetUriBaseType(Type type)
        {
            // TODO: Support this convention, not completely sure how it will work :/ [KNS]
            return this.wrappedFilter.GetUriBaseType(type);
        }


        public PropertyInfo GetParentToChildProperty(Type type)
        {
            return FromMappingOrDefault(type,
                x => x.ParentToChildProperty,
                () => wrappedFilter.GetParentToChildProperty(type));
        }


        public PropertyInfo GetChildToParentProperty(Type type)
        {
            return FromMappingOrDefault(type,
                x => x.ChildToParentProperty,
                () => wrappedFilter.GetChildToParentProperty(type));
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


        public bool PropertyFormulaIsDecompiled(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.PropertyFormulaIsDecompiled,
                () => this.wrappedFilter.PropertyFormulaIsDecompiled(propertyInfo));
        }


        public bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.AlwaysExpanded,
                () => this.wrappedFilter.PropertyIsAlwaysExpanded(propertyInfo));
        }


        public bool PropertyIsAttributes(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(
                propertyInfo,
                x => x.IsAttributesProperty,
                () => this.wrappedFilter.PropertyIsAttributes(propertyInfo));
        }


        public bool PropertyIsEtag(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo,
                x => x.IsEtagProperty,
                () => this.wrappedFilter.PropertyIsEtag(propertyInfo));
        }


        public bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            TypeMappingOptions typeMapping;
            PropertyMappingOptions propertyOptions;
            if (!TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMapping, out propertyOptions))
                return this.wrappedFilter.PropertyIsIncluded(propertyInfo);

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Excluded)
                return false;

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Included)
                return true;

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Default)
            {
                if (typeMapping.DefaultPropertyInclusionMode ==
                    DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault)
                    return this.wrappedFilter.PropertyIsIncluded(propertyInfo);

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


        public bool PropertyIsPrimaryId(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(
                propertyInfo,
                x => x.IsPrimaryKey,
                () => this.wrappedFilter.PropertyIsPrimaryId(propertyInfo));
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
            return FromMappingOrDefault(
                type,
                x => x.IsValueObject,
                () => this.wrappedFilter.TypeIsMappedAsValueObject(type));
        }


        internal TypeMappingOptions GetTypeMapping(Type type)
        {
            TypeMappingOptions typeMapping;

            return this.typeMappingDict.GetOrAdd(type.FullName,
                k =>
                {
                    typeMapping = new TypeMappingOptions(type);
                    typeMapping.DefaultPropertyInclusionMode = GetDefaultPropertyInclusionMode();
                    return typeMapping;
                });
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

        private class RuleMethod
        {
            private readonly Type appliesToType;

            public Type AppliesToType
            {
                get { return appliesToType; }
            }

            public MethodInfo Method
            {
                get { return method; }
            }

            public object Instance
            {
                get { return instance; }
            }

            private readonly MethodInfo method;
            private readonly object instance;

            public RuleMethod(MethodInfo method, object instance)
            {
                this.appliesToType = method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
                this.method = method;
                this.instance = instance;
            }


            public static MethodInfo getChildRulesMethod =
                ReflectionHelper.GetMethodDefinition<RuleMethod>(x => x.GetChildRules<object>());

            private IEnumerable<RuleMethod> GetChildRules<T>()
            {
                var typeConfigDelegates = new List<Delegate>();
                var nestedScanner = new NestedTypeMappingConfigurator<T>(typeConfigDelegates);
                Method.Invoke(Instance, new object[] { nestedScanner });
                return typeConfigDelegates.Select(x => new RuleMethod(x.Method, x.Target)).ToList();
            }

            public IEnumerable<RuleMethod> GetChildRules()
            {
                return (IEnumerable<RuleMethod>)getChildRulesMethod.MakeGenericMethod(AppliesToType).Invoke(this, null);
            }
        }


        private void ApplyRules(IEnumerable<RuleMethod> ruleMethods)
        {
            var allTransformedTypes = sourceTypes.Where(TypeIsMappedAsTransformedType).ToList();

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

        private static IEnumerable<RuleMethod> GetMappingRulesFromObjects(IEnumerable<object> ruleContainers)
        {
            if (ruleContainers == null)
                return Enumerable.Empty<RuleMethod>();
            return ruleContainers
                .SelectMany(
                    x => x.GetType()
                        .GetMethods()
                        .Where(IsRuleMethod)
                        .Select(
                            m => new RuleMethod(m, x)));
        }


        private T FromMappingOrDefault<T>(
            Type type,
            Func<TypeMappingOptions, T?> ifMappingExist,
            Func<T> ifMappingMissing)
            where T : struct
        {
            var result = FromMappingOrDefault(type, ifMappingExist, () => (T?)ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(
            Type type,
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


        private T FromMappingOrDefault<T>(
            PropertyInfo propertyInfo,
            Func<PropertyMappingOptions, T> ifMappingExist,
            Func<T> ifMappingMissing)
        {
            TypeMappingOptions typeMappingOptions;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMappingOptions, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T)result;
        }


        private T FromMappingOrDefault<T>(
            PropertyInfo propertyInfo,
            Func<PropertyMappingOptions, T?> ifMappingExist,
            Func<T> ifMappingMissing)
            where T : struct
        {
            TypeMappingOptions typeMappingOptions;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMappingOptions, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T)result;
        }


        private bool TryGetTypeMappingAndPropertyOptions(
            PropertyInfo propertyInfo,
            out TypeMappingOptions typeMapping,
            out PropertyMappingOptions propertyOptions)
        {
            typeMapping = GetTypeMapping(propertyInfo.ReflectedType);
            propertyOptions = typeMapping.GetPropertyOptions(propertyInfo);
            return true;
        }
    }
}