#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
            return $"{GetType().Name}({Uri}) - {(IsLoaded ? "loaded" : "not loaded")}";
        }


        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (Client == null)
            {
                throw new InvalidOperationException(
                    $"{this}.Initialize(IResourceFetchContext) must be invoked before OnGet.");
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
            throw new InvalidOperationException($"{property} is just a proxy. Use Patch to modify a resource.");
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