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


        private static bool IsRuleMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;
            var paramType = parameters[0].ParameterType;

            // Metadata token is the same across all generic type instances and generic type definition
            return paramType.MetadataToken == typeof (ITypeMappingConfigurator<>).MetadataToken;
        }


        private void ApplyRules(params object[] ruleContainers)
        {
            if (ruleContainers == null)
                throw new ArgumentNullException("ruleObj");

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
                TypeMappingConfigurator typeMapping;
                if (!typeMappingDict.TryGetValue(ruleMethod.AppliesToType.FullName, out typeMapping))
                {
                    var typeMappingConfiguratorType =
                        typeof (TypeMappingConfigurator<>).MakeGenericType(ruleMethod.AppliesToType);
                    typeMapping = (TypeMappingConfigurator) Activator.CreateInstance(typeMappingConfiguratorType);
                    typeMappingDict[ruleMethod.AppliesToType.FullName] = typeMapping;
                }
                ruleMethod.Method.Invoke(ruleMethod.Instance, new object[] {typeMapping});
            }
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

        #region Implementation of ITypeMappingFilter

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
            return wrappedFilter.GetUriBaseType(type);
        }


        public bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo)
        {
            return wrappedFilter.PropertyIsAlwaysExpanded(propertyInfo);
        }


        public bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            TypeMappingConfigurator typeMapping;
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
                {
                    return wrappedFilter.PropertyIsIncluded(propertyInfo);
                }
                else if (typeMapping.DefaultPropertyInclusionMode ==
                         DefaultPropertyInclusionMode.AllPropertiesRequiresExplicitMapping)
                {
                    throw new PomonaMappingException(
                        string.Format(
                            "All properties are required to be explicitly included or excluded from mapping, but {0} of {1} is neither.",
                            propertyInfo.Name, propertyInfo.DeclaringType.FullName));
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


        private bool FromMappingOrDefault(
            Type type, Func<TypeMappingConfigurator, bool?> ifMappingExist, Func<bool> ifMappingMissing)
        {
            var result = FromMappingOrDefault(type, ifMappingExist, () => (bool?) ifMappingMissing());
            if (!result.HasValue)
                throw new InvalidOperationException("Expected a non-null value here.");
            return result.Value;
        }


        private T FromMappingOrDefault<T>(
            Type type, Func<TypeMappingConfigurator, T> ifMappingExist, Func<T> ifMappingMissing)
        {
            TypeMappingConfigurator typeMappingConfigurator;
            object result = null;
            if (typeMappingDict.TryGetValue(type.FullName, out typeMappingConfigurator))
                result = ifMappingExist(typeMappingConfigurator);
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
            TypeMappingConfigurator typeMappingConfigurator;
            PropertyMappingOptions propertyOptions;
            object result = null;

            if (TryGetTypeMappingAndPropertyOptions(propertyInfo, out typeMappingConfigurator, out propertyOptions))
                result = ifMappingExist(propertyOptions);

            if (result == null)
                return ifMappingMissing();

            return (T) result;
        }


        private bool TryGetTypeMappingAndPropertyOptions(
            PropertyInfo propertyInfo,
            out TypeMappingConfigurator typeMapping,
            out PropertyMappingOptions propertyOptions)
        {
            propertyOptions = null;
            if (typeMappingDict.TryGetValue(propertyInfo.DeclaringType.FullName, out typeMapping))
            {
                propertyOptions = typeMapping.GetPropertyOptions(propertyInfo.Name);
            }

            return propertyOptions != null;
        }

        #endregion

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