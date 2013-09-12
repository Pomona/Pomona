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

using System.Collections.Generic;
using System.Reflection;
using Pomona.Internals;

namespace Pomona.Common.Proxies
{
    public class ClientSideFormProxyBase : PostResourceBase
    {
        private static readonly MethodInfo onGetAttributeMethod =
            ReflectionHelper.GetMethodDefinition<ClientSideFormProxyBase>(
                x => x.OnGetAttribute<object, object, object>(null));

        private static readonly MethodInfo onSetAttributeMethod =
            ReflectionHelper.GetMethodDefinition<ClientSideFormProxyBase>(
                x => x.OnSetAttribute<object, object, object>(null, null));

        public object ProxyTarget { get; internal set; }
        internal PropertyInfo AttributesProperty { get; set; }

        protected override TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (IsServerKnownProperty(property))
                return property.Get((TOwner) ProxyTarget);

            var dictValueType = AttributesProperty.PropertyType.GetGenericArguments()[1];
            return
                (TPropType)
                onGetAttributeMethod.MakeGenericMethod(typeof (TOwner), typeof (TPropType), dictValueType)
                                    .Invoke(this, new object[] {property});
        }

        private TPropType OnGetAttribute<TOwner, TPropType, TDictValue>(PropertyWrapper<TOwner, TPropType> property)
        {
            var dict = (IDictionary<string, TDictValue>) AttributesProperty.GetValue(ProxyTarget, null);
            return (TPropType) ((object) dict[property.Name]);
        }

        private bool OnSetAttribute<TOwner, TPropType, TDictValue>(PropertyWrapper<TOwner, TPropType> property,
                                                                   TPropType value)
        {
            var dict = (IDictionary<string, TDictValue>) AttributesProperty.GetValue(ProxyTarget, null);
            dict[property.Name] = (TDictValue) ((object) value);
            return false;
        }

        private bool IsServerKnownProperty<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return property.PropertyInfo.DeclaringType.IsInstanceOfType(ProxyTarget);
        }

        protected override void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            if (IsServerKnownProperty(property))
            {
                property.Set((TOwner) ProxyTarget, value);
                return;
            }

            var dictValueType = AttributesProperty.PropertyType.GetGenericArguments()[1];
            onSetAttributeMethod.MakeGenericMethod(typeof (TOwner), typeof (TPropType), dictValueType)
                                .Invoke(this, new object[] {property, value});
        }
    }
}