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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public static class PomonaClientExtensions
    {
        public static T Get<T>(this IPomonaClient client, string uri)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            return (T)client.Get(uri, typeof(T), null);
        }


        public static T Get<T>(this IPomonaClient client, string uri, RequestOptions requestOptions)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            return (T)client.Get(uri, typeof(T), requestOptions);
        }


        public static object Get(this IPomonaClient client, string uri, Type type)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            return client.Get(uri, type, null);
        }


        public static T GetLazy<T>(this IPomonaClient client, string uri)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            var typeInfo = client.GetResourceInfoForType(typeof(T));
            var proxy = (LazyProxyBase)Activator.CreateInstance(typeInfo.LazyProxyType);
            proxy.Initialize(uri, client, typeInfo.PocoType);
            return (T)((object)proxy);
        }


        public static T Patch<T>(this IPomonaClient client, T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            var patchForm = (T)client.TypeMapper.CreatePatchForm(typeof(T), target);
            updateAction(patchForm);

            var requestOptions = new RequestOptions<T>();
            if (options != null)
                options(requestOptions);

            return (T)client.Patch(patchForm, requestOptions);
        }


        public static IQueryable<T> Query<T>(this IPomonaRootResource client, Expression<Func<T, bool>> predicate)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            return client.Query<T>().Where(predicate);
        }


        public static IQueryable<TResource> Query<TResource>(this IQueryableRepository<TResource> repository,
                                                             Expression<Func<TResource, bool>> predicate)
            where TResource : class, IClientResource
        {
            return repository.Query().Where(predicate);
        }


        internal static Type GetResourceBaseInterface(this Type type)
        {
            return type.GetResourceInfoAttribute().BaseType;
        }


        internal static ResourceInfoAttribute GetResourceInfoAttribute(this Type type)
        {
            ResourceInfoAttribute ria;

            if (!type.TryGetResourceInfoAttribute(out ria))
                throw new InvalidOperationException("Unable to get resource info attribute");

            return ria;
        }


        internal static PropertyInfo GetResourceProperty(this Type type, string propertyName)
        {
            return
                type.WalkTree(x => x.GetResourceBaseInterface()).Select(x => x.GetProperty(propertyName)).FirstOrDefault
                    (x => x != null);
        }


        internal static object Post<T>(this IPomonaClient client, string uri, Action<T> postAction, RequestOptions options)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            var postForm = (T)client.TypeMapper.CreatePostForm(typeof(T));
            postAction(postForm);
            return client.Post(uri, postForm, options);
        }


        internal static bool TryGetResourceInfoAttribute(this Type type, out ResourceInfoAttribute resourceInfoAttribute)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            resourceInfoAttribute = type
                .GetCustomAttributes(typeof(ResourceInfoAttribute), false)
                .OfType<ResourceInfoAttribute>()
                .FirstOrDefault();

            return resourceInfoAttribute != null;
        }
    }
}