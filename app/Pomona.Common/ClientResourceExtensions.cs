#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public static class ClientResourceExtensions
    {
        public static T AddNew<T>(this ICollection<T> collection, Action<T> initAction)
            where T : IClientResource
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (initAction == null)
                throw new ArgumentNullException(nameof(initAction));
            var form = ClientTypeMapper.CreatePostForm<T>();
            initAction(form);
            collection.Add(form);
            return form;
        }


        public static bool IsLoaded<T>(this IEnumerable<T> collection)
            where T : IClientResource
        {
            var lazyProxy = collection as ILazyProxy;
            return lazyProxy == null || lazyProxy.IsLoaded;
        }


        public static bool IsLoaded(this IClientResource resource)
        {
            var lazyProxy = resource as ILazyProxy;
            return lazyProxy == null || lazyProxy.IsLoaded;
        }


        public static bool IsPersisted(this IClientResource resource)
        {
            return !IsTransient(resource);
        }


        public static bool IsTransient(this IClientResource resource)
        {
            return resource is IPostForm;
        }
    }
}

