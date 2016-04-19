#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public static class PomonaClientExtensions
    {
        public static T Get<T>(this IPomonaClient client, string uri)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            return (T)client.Get(uri, typeof(T), null);
        }


        public static T Get<T>(this IPomonaClient client, string uri, RequestOptions requestOptions)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            var type = typeof(T);
            var resource = client.Get(uri, type, requestOptions);

            if (resource == null && type.IsValueType && !type.IsNullable())
                throw new InvalidCastException($"The response from {uri} was null, which can't be cast to {type}.");

            return (T)resource;
        }


        public static object Get(this IPomonaClient client, string uri, Type type)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            return client.Get(uri, type, null);
        }


        public static async Task<T> GetAsync<T>(this IPomonaClient client, string uri, RequestOptions requestOptions)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            var type = typeof(T);
            var resource = await client.GetAsync(uri, type, requestOptions);

            if (resource == null && type.IsValueType && !type.IsNullable())
                throw new InvalidCastException($"The response from {uri} was null, which can't be cast to {type}.");

            return (T)resource;
        }


        public static T GetLazy<T>(this IPomonaClient client, string uri)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            var typeInfo = client.GetResourceInfoForType(typeof(T));
            var proxy = (LazyProxyBase)Activator.CreateInstance(typeInfo.LazyProxyType);
            proxy.Initialize(uri, client, typeInfo.PocoType);
            return (T)((object)proxy);
        }


        public static T Patch<T>(this IPomonaClient client, T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
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
                throw new ArgumentNullException(nameof(client));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
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


        internal static async Task<T> PatchAsync<T>(this IPomonaClient client,
                                                    T target,
                                                    Action<T> updateAction,
                                                    Action<IRequestOptions<T>> options = null)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            var patchForm = (T)client.TypeMapper.CreatePatchForm(typeof(T), target);
            updateAction(patchForm);

            var requestOptions = new RequestOptions<T>();
            options?.Invoke(requestOptions);

            return (T)await client.PatchAsync(patchForm, requestOptions);
        }


        internal static object Post<T>(this IPomonaClient client, string uri, Action<T> postAction, RequestOptions options)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            var postForm = (T)client.TypeMapper.CreatePostForm(typeof(T));
            postAction(postForm);
            return client.Post(uri, postForm, options);
        }


        internal static Task<object> PostAsync<T>(this IPomonaClient client, string uri, Action<T> postAction, RequestOptions options)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            var postForm = client.TypeMapper.CreatePostForm(typeof(T));
            postAction((T)postForm);
            return client.PostAsync(uri, postForm, options);
        }


        internal static bool TryGetResourceInfoAttribute(this Type type, out ResourceInfoAttribute resourceInfoAttribute)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            resourceInfoAttribute = type
                .GetCustomAttributes(typeof(ResourceInfoAttribute), false)
                .OfType<ResourceInfoAttribute>()
                .FirstOrDefault();

            return resourceInfoAttribute != null;
        }
    }
}