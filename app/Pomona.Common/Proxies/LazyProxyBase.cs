#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using Pomona.Common.Loading;

namespace Pomona.Common.Proxies
{
    public class LazyProxyBase : IHasResourceUri, ILazyProxy
    {
        private string expandPath;
        public IResourceLoader Client { get; internal set; }
        public object ProxyTarget { get; internal set; }
        public Type ProxyTargetType { get; private set; }


        public override string ToString()
        {
            return string.Format("{0}({1}) - {2}", GetType().Name, Uri, IsLoaded ? "loaded" : "not loaded");
        }


        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (Client == null)
            {
                throw new InvalidOperationException(
                    String.Format("{0}.Initialize(IResourceFetchContext) must be invoked before OnGet.", this));
            }

            try
            {
                Fetch();
            }
            catch (LoadException exception)
            {
                var resourcePath = this.expandPath ?? property.ToString();
                throw new LazyLoadingDisabledException(resourcePath, exception);
            }

            return property.Getter((TOwner)ProxyTarget);
        }


        protected void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new InvalidOperationException(String.Format("{0} is just a proxy. Use Patch to modify a resource.",
                                                              property));
        }


        internal void Initialize(string uri,
                                 IResourceLoader resourceLoader,
                                 Type proxyTargetType,
                                 string expandPath = null)
        {
            if (String.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            if (resourceLoader == null)
                throw new ArgumentNullException(nameof(resourceLoader));

            if (proxyTargetType == null)
                throw new ArgumentNullException(nameof(proxyTargetType));

            Uri = uri;
            Client = resourceLoader;
            ProxyTargetType = proxyTargetType;
            this.expandPath = expandPath;
        }


        private void Fetch()
        {
            if (IsLoaded)
                return;

            ProxyTarget = Client.Get(Uri, ProxyTargetType);
            var hasResourceUri = ProxyTarget as IHasResourceUri;
            if (hasResourceUri != null)
                Uri = hasResourceUri.Uri;
        }


        public bool IsLoaded
        {
            get { return ProxyTarget != null; }
        }

        public string Uri { get; private set; }
    }
}