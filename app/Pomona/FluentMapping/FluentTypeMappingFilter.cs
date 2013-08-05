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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    public sealed class FluentTypeMappingFilter : ITypeMappingFilter
    {
        private readonly IDictionary<string, TypeMappingOptions> typeMappingDict =
            new Dictionary<string, TypeMappingOptions>();

        private readonly ITypeMappingFilter wrappedFilter;


        public FluentTypeMappingFilter(ITypeMappingFilter wrappedFilter, params object[] fluentRuleObjects)
        {
            this.wrappedFilter = wrappedFilter;

            foreach (var ruleObj in fluentRuleObjects)
                ApplyRules(ruleObj);
        }


        public string ApiVersion
        {
            get { return wrappedFilter.ApiVersion; }
        }


        public bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            return wrappedFilter.ClientPropertyIsExposedAsRepository(propertyInfo);
        }


        public string GetClientAssemblyName()
        {
            return wrappedFilter.GetClientAssemblyName();
        }


        public Type GetClientLibraryType(Type type)
        {
            return wrappedFilter.GetClientLibraryType(type);
        }

        public bool IsIndependentTypeRoot(Type type)
        {
            return FromMappingOrDefault(type, tmo => tmo.IsIndependentTypeRoot,
                                        () => wrappedFilter.IsIndependentTypeRoot(type));
        }


        public DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return wrappedFilter.GetDefaultPropertyInclusionMode();
        }


        public object GetIdFor(object entity)
        {
            return wrappedFilter.GetIdFor(entity);
        }


        public JsonConverter GetJsonConverterForType(Type type)
        {
            return wrappedFilter.GetJsonConverterForType(type);
        }


        public PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty)
        {
            return wrappedFilter.GetOneToManyCollectionForeignKey(collectionProperty);
        }


        public Type GetPostReturnType(Type type)
        {
            return FromMappingOrDefault(
                type,
                x => x.PostResponseType ?? wrappedFilter.GetPostReturnType(type),
                () => wrappedFilter.GetPostReturnType(type));
        }


        public Func<object, object> GetPropertyGetter(PropertyInfo propertyInfo)
        {
            return wrappedFilter.GetPropertyGetter(propertyInfo);
        }


        public string GetPropertyMappedName(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(
                propertyInfo, x => x.Name, () => wrappedFilter.GetPropertyMappedName(propertyInfo));
        }


        public Action<object, object> GetPropertySetter(PropertyInfo propertyInfo)
        {
            return wrappedFilter.GetPropertySetter(propertyInfo);
        }

        public LambdaExpression GetPropertyFormula(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo, x => x.Formula,
                                        () => wrappedFilter.GetPropertyFormula(propertyInfo));
        }

        public bool PropertyFormulaIsDecompiled(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo, x => x.PropertyFormulaIsDecompiled,
                                        () => wrappedFilter.PropertyFormulaIsDecompiled(propertyInfo));
        }

        public LambdaExpression GetDecompiledPropertyFormula(PropertyInfo propertyInfo)
        {
            return wrappedFilter.GetDecompiledPropertyFormula(propertyInfo);
        }

        public bool PropertyIsEtag(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo, x => x.IsEtagProperty,
                                        () => wrappedFilter.PropertyIsEtag(propertyInfo));
        }

        public string GetPluralNameForType(Type type)
        {
            return FromMappingOrDefault(type, x => x.PluralName, () => wrappedFilter.GetPluralNameForType(type));
        }

        public PropertyCreateMode GetPropertyCreateMode(PropertyInfo propertyInfo, ParameterInfo ctorParameterInfo)
        {
            return FromMappingOrDefault(propertyInfo, x => x.CreateMode,
                                        () => wrappedFilter.GetPropertyCreateMode(propertyInfo, ctorParameterInfo));
        }

        public PropertyAccessMode GetPropertyAccessMode(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(propertyInfo, x => x.AccessMode,
                                        () => wrappedFilter.GetPropertyAccessMode(propertyInfo));
        }

        public Type GetPropertyType(PropertyInfo propertyInfo)
        {
            return wrappedFilter.GetPropertyType(propertyInfo);
        }


        public IEnumerable<Type> GetSourceTypes()
        {
            return wrappedFilter.GetSourceTypes();
        }


        public ConstructorInfo GetTypeConstructor(Type type)
        {
            return FromMappingOrDefault(type, x => x.Constructor, () => wrappedFilter.GetTypeConstructor(type));
        }


        public Type GetUriBaseType(Type type)
        {
            // TODO: Support this convention, not completely sure how it will work :/ [KNS]
            return wrappedFilter.GetUriBaseType(type);
        }


        public bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo)
        {
            return wrappedFilter.PropertyIsAlwaysExpanded(propertyInfo);
        }


        public bool TypeIsExposedAsRepository(Type type)
        {
            return FromMappingOrDefault(type, x => x.IsUriBaseType, () => wrappedFilter.TypeIsExposedAsRepository(type));
        }

        public bool PropertyIsAttributes(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(
                propertyInfo, x => x.IsAttributesProperty, () => wrappedFilter.PropertyIsAttributes(propertyInfo));
        }


        public bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            TypeMappingOptions typeMapping;
            PropertyMappingOptions propertyOptions;
            if (!TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMapping, out propertyOptions))
                return wrappedFilter.PropertyIsIncluded(propertyInfo);

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Excluded)
                return false;

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Included)
                return true;

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Default)
            {
                if (typeMapping.DefaultPropertyInclusionMode ==
                    DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault)
                    return wrappedFilter.PropertyIsIncluded(propertyInfo);

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
                () => wrappedFilter.PropertyIsPrimaryId(propertyInfo));
        }


        public Type ResolveRealTypeForProxy(Type type)
        {
            return wrappedFilter.ResolveRealTypeForProxy(type);
        }


        public bool TypeIsMapped(Type type)
        {
            return wrappedFilter.TypeIsMapped(type);
        }


        public bool TypeIsMappedAsCollection(Type type)
        {
            return wrappedFilter.TypeIsMappedAsCollection(type);
        }


        public bool TypeIsMappedAsSharedType(Type type)
        {
            return wrappedFilter.TypeIsMappedAsSharedType(type);
        }


        public bool TypeIsMappedAsTransformedType(Type type)
        {
            return wrappedFilter.TypeIsMappedAsTransformedType(type);
        }


        public bool TypeIsMappedAsValueObject(Type type)
        {
            return FromMappingOrDefault(
                type, x => x.IsValueObject, () => wrappedFilter.TypeIsMappedAsValueObject(type));
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


        internal TypeMappingOptions GetTypeMapping(Type type)
        {
            TypeMappingOptions typeMapping;
            if (!typeMappingDict.TryGetValue(type.FullName, out typeMapping))
            {
                typeMapping = new TypeMappingOptions(type);
                typeMapping.DefaultPropertyInclusionMode = GetDefaultPropertyInclusionMode();
                typeMappingDict[type.FullName] = typeMapping;
            }

            return typeMapping;
        }


        private static bool IsRuleMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;
            var paramType = parameters[0].ParameterType;

            // Metadata token is the same across all generic type instances and generic type definition
            return paramType.UniqueToken() == typeof (ITypeMappingConfigurator<>).UniqueToken();
        }


        private void ApplyRules(params object[] ruleContainers)
        {
            if (ruleContainers == null)
                throw new ArgumentNullException("ruleContainers");

            var allTransformedTypes = GetSourceTypes().Where(TypeIsMappedAsTransformedType).ToList();

            // Find all rule methods in all instances
            var ruleMethods = ruleContainers
                .SelectMany(
                    x => x.GetType()
                          .GetMethods()
                          .Where(IsRuleMethod)
                          .Select(
                              m => new
                                  {
                                      Method = m,
                                      Instance = x,
                                      AppliesToType = m.GetParameters()[0].ParameterType.GetGenericArguments()[0]
                                  }));

            // NOTE: We need to order the properties in ascending order by how
            //       specific their declaring types are so we get the most
            //       specific ones last.
            ruleMethods = ruleMethods.OrderBy(x => x.AppliesToType, new SubclassComparer());

            foreach (var ruleMethod in ruleMethods)
            {
                var appliesToType = ruleMethod.AppliesToType;
                foreach (var subType in allTransformedTypes.Where(x => appliesToType.IsAssignableFrom(x)))
                {
                    var typeMapping = GetTypeMapping(subType);
                    var configurator = typeMapping.GetConfigurator(ruleMethod.AppliesToType);
                    ruleMethod.Method.Invoke(ruleMethod.Instance, new[] {configurator});
                }
            }
        }


        private bool FromMappingOrDefault(
            Type type, Func<TypeMappingOptions, bool?> ifMappingExist, Func<bool> ifMappingMissing)
        {
            var result = FromMappingOrDefault(type, ifMappingExist, () => (bool?) ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(
            Type type, Func<TypeMappingOptions, T> ifMappingExist, Func<T> ifMappingMissing)
        {
            TypeMappingOptions typeMappingOptions;
            object result = null;
            if (typeMappingDict.TryGetValue(type.FullName, out typeMappingOptions))
                result = ifMappingExist(typeMappingOptions);
            if (result == null)
                return ifMappingMissing();
            return (T) result;
        }


        private bool FromMappingOrDefault(
            PropertyInfo propertyInfo, Func<PropertyMappingOptions, bool?> ifMappingExist, Func<bool> ifMappingMissing)
        {
            var result = FromMappingOrDefault(propertyInfo, ifMappingExist, () => (bool?) ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(
            PropertyInfo propertyInfo, Func<PropertyMappingOptions, T> ifMappingExist, Func<T> ifMappingMissing)
        {
            TypeMappingOptions typeMappingOptions;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMappingOptions, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T) result;
        }


        private T FromMappingOrDefault<T>(
            PropertyInfo propertyInfo, Func<PropertyMappingOptions, T?> ifMappingExist, Func<T> ifMappingMissing)
            where T : struct
        {
            TypeMappingOptions typeMappingOptions;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMappingOptions, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T) result;
        }

        private bool TryGetTypeMappingAndPropertyOptions(
            PropertyInfo propertyInfo,
            out TypeMappingOptions typeMapping,
            out PropertyMappingOptions propertyOptions)
        {
            typeMapping = GetTypeMapping(propertyInfo.ReflectedType);
            propertyOptions = typeMapping.GetPropertyOptions(propertyInfo.Name);
            return true;
        }

        #region Nested type: SubclassComparer

        private class SubclassComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                if (x == y)
                    return 0;

                return x.IsAssignableFrom(y)
                           ? -1
                           : 1;
            }
        }

        #endregion
    }
}