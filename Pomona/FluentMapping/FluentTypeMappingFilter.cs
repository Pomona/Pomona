using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;

namespace Pomona.FluentMapping
{
    public sealed class FluentTypeMappingFilter : ITypeMappingFilter
    {
        private readonly IDictionary<string, TypeMappingConfigurator> typeMappingDict =
            new Dictionary<string, TypeMappingConfigurator>();

        private readonly ITypeMappingFilter wrappedFilter;


        public FluentTypeMappingFilter(ITypeMappingFilter wrappedFilter, params object[] fluentRuleObjects)
        {
            this.wrappedFilter = wrappedFilter;

            foreach (var ruleObj in fluentRuleObjects)
                ApplyRules(ruleObj);
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
            return this.wrappedFilter.ClientPropertyIsExposedAsRepository(propertyInfo);
        }


        public string GetClientAssemblyName()
        {
            return this.wrappedFilter.GetClientAssemblyName();
        }


        public Type GetClientLibraryType(Type type)
        {
            return this.wrappedFilter.GetClientLibraryType(type);
        }


        public DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return this.wrappedFilter.GetDefaultPropertyInclusionMode();
        }


        public object GetIdFor(object entity)
        {
            return this.wrappedFilter.GetIdFor(entity);
        }


        public JsonConverter GetJsonConverterForType(Type type)
        {
            return this.wrappedFilter.GetJsonConverterForType(type);
        }


        public PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty)
        {
            return this.wrappedFilter.GetOneToManyCollectionForeignKey(collectionProperty);
        }


        public Type GetPostReturnType(Type type)
        {
            return FromMappingOrDefault(
                type,
                x => x.PostResponseType ?? this.wrappedFilter.GetPostReturnType(type),
                () => this.wrappedFilter.GetPostReturnType(type));
        }


        public Func<object, object> GetPropertyGetter(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyGetter(propertyInfo);
        }


        public string GetPropertyMappedName(PropertyInfo propertyInfo)
        {
            return FromMappingOrDefault(
                propertyInfo, x => x.Name, () => this.wrappedFilter.GetPropertyMappedName(propertyInfo));
        }


        public Action<object, object> GetPropertySetter(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertySetter(propertyInfo);
        }


        public Type GetPropertyType(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyType(propertyInfo);
        }


        public IEnumerable<Type> GetSourceTypes()
        {
            return this.wrappedFilter.GetSourceTypes();
        }


        public ConstructorInfo GetTypeConstructor(Type type)
        {
            return FromMappingOrDefault(type, x => x.Constructor, () => this.wrappedFilter.GetTypeConstructor(type));
        }


        public Type GetUriBaseType(Type type)
        {
            return this.wrappedFilter.GetUriBaseType(type);
        }


        public bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsAlwaysExpanded(propertyInfo);
        }


        public bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            TypeMappingConfigurator typeMapping;
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
                type, x => x.IsValueObject, () => this.wrappedFilter.TypeIsMappedAsValueObject(type));
        }


        internal TypeMappingConfigurator GetTypeMapping(Type type)
        {
            TypeMappingConfigurator typeMapping;
            if (!this.typeMappingDict.TryGetValue(type.FullName, out typeMapping))
            {
                var typeMappingConfiguratorType =
                    typeof(TypeMappingConfigurator<>).MakeGenericType(type);
                typeMapping = (TypeMappingConfigurator)Activator.CreateInstance(typeMappingConfiguratorType);
                typeMapping.DefaultPropertyInclusionMode = GetDefaultPropertyInclusionMode();
                this.typeMappingDict[type.FullName] = typeMapping;
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
            return paramType.MetadataToken == typeof(ITypeMappingConfigurator<>).MetadataToken;
        }


        private void ApplyRules(params object[] ruleContainers)
        {
            if (ruleContainers == null)
                throw new ArgumentNullException("ruleContainers");

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
                var typeMapping = GetTypeMapping(ruleMethod.AppliesToType);
                ruleMethod.Method.Invoke(ruleMethod.Instance, new object[] { typeMapping });
            }
        }


        private bool FromMappingOrDefault(
            Type type, Func<TypeMappingConfigurator, bool?> ifMappingExist, Func<bool> ifMappingMissing)
        {
            var result = FromMappingOrDefault(type, ifMappingExist, () => (bool?)ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(
            Type type, Func<TypeMappingConfigurator, T> ifMappingExist, Func<T> ifMappingMissing)
        {
            TypeMappingConfigurator typeMappingConfigurator;
            object result = null;
            if (this.typeMappingDict.TryGetValue(type.FullName, out typeMappingConfigurator))
                result = ifMappingExist(typeMappingConfigurator);
            if (result == null)
                return ifMappingMissing();
            return (T)result;
        }


        private bool FromMappingOrDefault(
            PropertyInfo propertyInfo, Func<PropertyMappingOptions, bool?> ifMappingExist, Func<bool> ifMappingMissing)
        {
            var result = FromMappingOrDefault(propertyInfo, ifMappingExist, () => (bool?)ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(
            PropertyInfo propertyInfo, Func<PropertyMappingOptions, T> ifMappingExist, Func<T> ifMappingMissing)
        {
            TypeMappingConfigurator typeMappingConfigurator;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMappingConfigurator, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T)result;
        }


        private bool TryGetTypeMappingAndPropertyOptions(
            PropertyInfo propertyInfo,
            out TypeMappingConfigurator typeMapping,
            out PropertyMappingOptions propertyOptions)
        {
            typeMapping = GetTypeMapping(propertyInfo.DeclaringType);
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