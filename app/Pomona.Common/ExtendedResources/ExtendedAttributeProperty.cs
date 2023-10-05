#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal abstract class ExtendedAttributeProperty : ExtendedProperty
    {
        private static readonly MethodInfo createMethod =
            ReflectionHelper.GetMethodDefinition(() => Create<object, object>(null, null));


        protected ExtendedAttributeProperty(PropertyInfo property)
            : base(property)
        {
        }


        public static ExtendedProperty Create(PropertyInfo property, ExtendedResourceInfo declaringTypeInfo)
        {
            return
                (ExtendedAttributeProperty)
                    createMethod.MakeGenericMethod(declaringTypeInfo.DictValueType, property.PropertyType)
                                .Invoke(null, new object[] { property, declaringTypeInfo });
        }


        private static ExtendedProperty Create<TDictValue, TProperty>(PropertyInfo property,
                                                                      ExtendedResourceInfo declaringTypeInfo)
        {
            return new ExtendedAttributeProperty<TDictValue, TProperty>(property, declaringTypeInfo);
        }
    }

    internal class ExtendedAttributeProperty<TDictValue, TProperty> : ExtendedAttributeProperty
    {
        private readonly ExtendedResourceInfo declaringTypeInfo;
        private readonly string key;


        public ExtendedAttributeProperty(PropertyInfo property,
                                         ExtendedResourceInfo declaringTypeInfo,
                                         string key = null)
            : base(property)
        {
            this.declaringTypeInfo = declaringTypeInfo;
            this.key = key ?? property.Name;
        }


        public override object GetValue(object obj, IDictionary<string, IExtendedResourceProxy> cache)
        {
            TDictValue value;
            if (GetDictionary(obj).TryGetValue(this.key, out value))
                return value;
            return null;
        }


        public override void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache)
        {
            GetDictionary(obj)[this.key] = (TDictValue)value;
        }


        private IDictionary<string, TDictValue> GetDictionary(object obj)
        {
            return (IDictionary<string, TDictValue>)this.declaringTypeInfo.DictProperty.GetValue(obj, null);
        }
    }
}
