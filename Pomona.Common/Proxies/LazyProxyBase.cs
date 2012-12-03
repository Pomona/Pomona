#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

namespace Pomona.Common.Proxies
{
    public class LazyProxyBase : IHasResourceUri
    {
        private ClientBase client;

        private object proxyTarget;
        private Type proxyTargetType;

        private string uri;

        public ClientBase Client
        {
            get { return client; }
            internal set { client = value; }
        }

        public object ProxyTarget
        {
            get { return proxyTarget; }
            internal set { proxyTarget = value; }
        }

        public Type ProxyTargetType
        {
            get { return proxyTargetType; }
            internal set { proxyTargetType = value; }
        }

        #region IHasResourceUri Members

        string IHasResourceUri.Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        #endregion

        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (proxyTarget == null)
                proxyTarget = client.GetUri(uri, proxyTargetType);

            return property.Getter((TOwner) proxyTarget);
        }


        protected object OnPropertyGet(string propertyName)
        {
            if (proxyTarget == null)
                proxyTarget = client.GetUri(uri, proxyTargetType);

            // TODO: Optimize this, maybe OnPropertyGet could provide a lambda to return the prop value from an interface.
            return proxyTargetType.GetProperty(propertyName).GetValue(proxyTarget, null);
        }


        protected void OnPropertySet(string propertyName, object value)
        {
            throw new NotImplementedException();
        }


        protected void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotImplementedException();
        }
    }
}