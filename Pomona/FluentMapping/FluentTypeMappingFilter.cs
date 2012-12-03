using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            {
                ApplyRules(ruleObj);
            }
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

        private void ApplyRules(params object[] ruleContainers)
        {
            if (ruleContainers == null)
                throw new ArgumentNullException("ruleObj");

            // Find all rule methods in all instances
            var ruleMethods = ruleContainers
                .SelectMany(x => x.GetType()
                                  .GetMethods()
                                  .Where(IsRuleMethod)
                                  .Select(m => new
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


        private static bool IsRuleMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;
            var paramType = parameters[0].ParameterType;

            // Metadata token is the same across all generic type instances and generic type definition
            return paramType.MetadataToken == typeof (ITypeMappingConfigurator<>).MetadataToken;
        }

        #region Implementation of ITypeMappingFilter

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

            if (propertyOptions.InclusionMode == PropertyInclusionMode.Default &&
                typeMapping.DefaultPropertyInclusionMode
                == DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault)
                return wrappedFilter.PropertyIsIncluded(propertyInfo);

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
            return typeMappingDict.TryGetValue(propertyInfo.ReflectedType.FullName, out typeMapping) &&
                   typeMapping.PropertyOptions.TryGetValue(propertyInfo.Name, out propertyOptions);
        }

        #endregion
    }
}