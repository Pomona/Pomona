#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
        private object proxyTarget;
        private Type proxyTargetType;
        private IResourceLoader resourceLoader;
        private string uri;

        public IResourceLoader Client
        {
            get { return this.resourceLoader; }
            internal set { this.resourceLoader = value; }
        }

        public bool IsLoaded
        {
            get { return this.proxyTarget != null; }
        }

        public object ProxyTarget
        {
            get { return this.proxyTarget; }
            internal set { this.proxyTarget = value; }
        }

        public Type ProxyTargetType
        {
            get { return this.proxyTargetType; }
        }

        public string Uri
        {
            get { return this.uri; }
        }


        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (this.resourceLoader == null)
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

            return property.Getter((TOwner)this.proxyTarget);
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
                throw new ArgumentNullException("uri");

            if (resourceLoader == null)
                throw new ArgumentNullException("resourceFetcher");

            if (proxyTargetType == null)
                throw new ArgumentNullException("proxyTargetType");

            this.uri = uri;
            this.resourceLoader = resourceLoader;
            this.proxyTargetType = proxyTargetType;
            this.expandPath = expandPath;
        }


        private void Fetch()
        {
            if (IsLoaded)
                return;

            this.proxyTarget = this.resourceLoader.Get(this.uri, this.proxyTargetType);
            var hasResourceUri = this.proxyTarget as IHasResourceUri;
            if (hasResourceUri != null)
                this.uri = hasResourceUri.Uri;
        }
    }
}