#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class ObjectDeltaProxyBase : ObjectDelta, IPomonaSerializable
    {
        protected ObjectDeltaProxyBase()
        {
        }


        public static object CreateDeltaProxy(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent, Type propertyType)
        {
            if (type.SerializationMode == TypeSerializationMode.Structured)
                return Create(original, type, typeMapper, parent);
            if (type.IsCollection)
                return CollectionDelta.CreateTypedCollectionDelta(original, type, typeMapper, parent, propertyType);
            if (type.IsDictionary)
            {
                var dictType = (DictionaryTypeSpec)type;

                var dictTypeInstance = typeof(DictionaryDelta<,,>).MakeGenericType(dictType.KeyType, dictType.ValueType, type);
                return Activator.CreateInstance(dictTypeInstance, original, type, typeMapper, parent);
            }
            throw new NotImplementedException();
        }


        protected override object CreateNestedDelta(object propValue, TypeSpec propValueType, Type propertyType)
        {
            return CreateDeltaProxy(propValue, propValueType, TypeMapper, this, propertyType);
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return (TPropType)GetPropertyValue(property.Name);
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            SetPropertyValue(property.Name, value);
        }


        private static object Create(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent)
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else
            var proxy =
                RuntimeProxyFactory.Create(typeof(ObjectDeltaProxyBase<>).MakeGenericType(type.Type),
                                           type.Type);
            var odpb = (ObjectDeltaProxyBase)proxy;
            odpb.Original = original;
            odpb.Type = type;
            odpb.TypeMapper = typeMapper;
            odpb.Parent = parent;
            return proxy;
#endif
        }


        public bool PropertyIsSerialized(string propertyName)
        {
            object propValue;
            return TrackedProperties.TryGetValue(propertyName, out propValue) && ValueIsDirty(propValue);
        }
    }

    public class ObjectDeltaProxyBase<T> : ObjectDeltaProxyBase, IDelta<T>
        where T : class
    {
        public new T Original => (T)base.Original;
    }
}