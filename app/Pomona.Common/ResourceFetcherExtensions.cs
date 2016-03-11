#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.Loading;

namespace Pomona.Common
{
    public static class ResourceFetcherExtensions
    {
        public static T Get<T>(this IResourceLoader resourceLoader, string uri)
        {
            if (resourceLoader == null)
                throw new ArgumentNullException("client");

            return (T)resourceLoader.Get(uri, typeof(T), null);
        }


        public static T Get<T>(this IResourceLoader resourceLoader, string uri, RequestOptions requestOptions)
        {
            if (resourceLoader == null)
                throw new ArgumentNullException("client");

            return (T)resourceLoader.Get(uri, typeof(T), requestOptions);
        }


        public static object Get(this IResourceLoader resourceLoader, string uri, Type type)
        {
            if (resourceLoader == null)
                throw new ArgumentNullException("client");

            return resourceLoader.Get(uri, type, null);
        }
    }
}