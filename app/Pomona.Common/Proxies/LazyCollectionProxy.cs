#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

using Pomona.Common.Loading;

namespace Pomona.Common.Proxies
{
    public abstract class LazyCollectionProxy : ILazyProxy, IHasResourceUri
    {
        protected LazyCollectionProxy(string uri, IResourceLoader resourceLoader)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            Uri = uri;
            ResourceLoader = resourceLoader;
        }


        protected IResourceLoader ResourceLoader { get; }


        internal static object CreateForType(Type collectionType, string uri, IPomonaClient clientBase)
        {
            Type[] genArgs;
            if (collectionType.TryExtractTypeArguments(typeof(ISet<>), out genArgs))
                return Activator.CreateInstance(typeof(LazySetProxy<>).MakeGenericType(genArgs), uri, clientBase);

            if (collectionType.TryExtractTypeArguments(typeof(IEnumerable<>), out genArgs))
                return Activator.CreateInstance(typeof(LazyListProxy<>).MakeGenericType(genArgs), uri, clientBase);

            throw new NotSupportedException("Unable to create lazy list proxy for collection type " + collectionType);
        }


        public abstract bool IsLoaded { get; }

        public string Uri { get; }
    }
}
