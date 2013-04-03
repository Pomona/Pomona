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
using System.Reflection;
using Pomona.Common.Internals;

namespace Pomona.Common.Proxies
{
    public class ClientSideResourceProxyBase : IHasResourceUri
    {
        public object ProxyTarget { get; internal set; }
        internal PropertyInfo AttributesProperty { get; set; }

        string IHasResourceUri.Uri
        {
            get { return ((IHasResourceUri) ProxyTarget).Uri; }
            set { throw new NotSupportedException(); }
        }


        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (property.PropertyInfo.DeclaringType.IsInstanceOfType(ProxyTarget))
                return property.Getter((TOwner) ProxyTarget);

            var dictTypeInstance =
                AttributesProperty.PropertyType.GetInterfacesOfGeneric(typeof (IDictionary<,>)).First();
            var dict = AttributesProperty.GetValue(ProxyTarget, null);
            var attrKey = property.PropertyInfo.Name;
            return (TPropType)
                   OdataFunctionMapping.SafeGetMethod.MakeGenericMethod(dictTypeInstance.GetGenericArguments())
                                       .Invoke(null, new[] {dict, attrKey});
        }


        protected void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotImplementedException();
        }
    }
}