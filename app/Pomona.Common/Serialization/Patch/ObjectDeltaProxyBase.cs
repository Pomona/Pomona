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
using System.Reflection;

using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class ObjectDeltaProxyBase : ObjectDelta, IPomonaSerializable
    {
        protected ObjectDeltaProxyBase()
        {
        }

        public bool PropertyIsSerialized(string propertyName)
        {
            object propValue;
            return TrackedProperties.TryGetValue(propertyName, out propValue) && ValueIsDirty(propValue);
        }

        private static object Create(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent)
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else
            var proxy =
                RuntimeProxyFactory.Create(typeof (ObjectDeltaProxyBase<>).MakeGenericType(type.Type),
                                           type.Type);
            var odpb = (ObjectDeltaProxyBase) proxy;
            odpb.Original = original;
            odpb.Type = type;
            odpb.TypeMapper = typeMapper;
            odpb.Parent = parent;
            return proxy;
#endif
        }

        public static object CreateDeltaProxy(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent, Type propertyType)
        {
            if (type.SerializationMode == TypeSerializationMode.Complex)
            {
                return Create(original, type, typeMapper, parent);
            }
            if (type.IsCollection)
                return CollectionDelta.CreateTypedCollectionDelta(original, type, typeMapper, parent, propertyType);
            if (type.IsDictionary)
            {
                var dictType = (DictionaryTypeSpec)type;

                var dictTypeInstance = typeof (DictionaryDelta<,,>).MakeGenericType(dictType.KeyType, dictType.ValueType, type);
                return Activator.CreateInstance(dictTypeInstance, original, type, typeMapper, parent);
            }
            throw new NotImplementedException();
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return (TPropType) GetPropertyValue(property.Name);
        }

        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            SetPropertyValue(property.Name, value);
        }

        protected override object CreateNestedDelta(object propValue, TypeSpec propValueType, Type propertyType)
        {
            return CreateDeltaProxy(propValue, propValueType, TypeMapper, this, propertyType);
        }
    }

    public class ObjectDeltaProxyBase<T> : ObjectDeltaProxyBase, IDelta<T>
        where T : class
    {
        public new T Original
        {
            get { return (T)base.Original; }
        }
    }
}