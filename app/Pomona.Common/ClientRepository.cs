#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Pomona.Common.Linq;
using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public class ClientRepository<TResource, TPostResponseResource, TId>
        :
            IClientRepository<TResource, TPostResponseResource, TId>,
            IQueryable<TResource>,
            IGettableRepository<TResource, TId>,
            IDeletableByIdRepository<TId>
        where TResource : class, IClientResource
        where TPostResponseResource : IClientResource
    {
        private readonly IEnumerable<TResource> results;


        public ClientRepository(IPomonaClient client, string uri, IEnumerable results, IClientResource parent)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            Client = client;
            Uri = uri;
            this.results = results as IEnumerable<TResource> ?? (results != null ? results.Cast<TResource>() : null);
        }


        internal IPomonaClient Client { get; }


        private string GetResourceUri(object id)
        {
            return $"{Uri}/{HttpUtility.UrlPathSegmentEncode(Convert.ToString(id, CultureInfo.InvariantCulture))}";
        }


        public virtual void Delete(TResource resource)
        {
            Client.Delete(resource, null);
        }


        public virtual Task DeleteAsync(TResource resource)
        {
            return Client.DeleteAsync(resource, null);
        }


        public Type ElementType
        {
            get { return typeof(TResource); }
        }

        public Expression Expression
        {
            get { return Expression.Constant(Query()); }
        }


        public TResource Get(TId id)
        {
            return Client.Get<TResource>(GetResourceUri(id));
        }


        public IEnumerator<TResource> GetEnumerator()
        {
            return this.results != null ? this.results.GetEnumerator() : Query().GetEnumerator();
        }


        public TResource GetLazy(TId id)
        {
            return Client.GetLazy<TResource>(GetResourceUri(id));
        }


        public TSubResource Patch<TSubResource>(TSubResource resource,
                                                Action<TSubResource> patchAction,
                                                Action<IRequestOptions<TSubResource>> options) where TSubResource : class, TResource
        {
            return Client.Patch(resource, patchAction, options);
        }


        public TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction)
            where TSubResource : class, TResource
        {
            return Patch(resource, patchAction, null);
        }


        public Task<TSubResource> PatchAsync<TSubResource>(TSubResource resource,
                                                           Action<TSubResource> patchAction,
                                                           Action<IRequestOptions<TSubResource>> options)
            where TSubResource : class, TResource
        {
            return Client.PatchAsync(resource, patchAction, options);
        }


        public virtual TPostResponseResource Post(IPostForm form)
        {
            return (TPostResponseResource)Client.Post(Uri, (TResource)((object)form), null);
        }


        public virtual TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource
        {
            return (TPostResponseResource)Client.Post<TSubResource>(Uri, postAction, null);
        }


        public virtual TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                                     Action<IRequestOptions<TSubResponseResource>> options)
            where TSubResource : class, TResource
            where TSubResponseResource : TPostResponseResource
        {
            var requestOptions = RequestOptions.Create(options, typeof(TSubResponseResource));
            return (TSubResponseResource)Client.Post<TSubResource>(Uri, postAction, requestOptions);
        }


        public virtual TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction,
                                                                Action<IRequestOptions<TPostResponseResource>> options)
            where TSubResource : class, TResource
        {
            return
                (TPostResponseResource)
                    Client.Post<TSubResource>(Uri, postAction, RequestOptions.Create(options));
        }


        public virtual TPostResponseResource Post(Action<TResource> postAction)
        {
            return (TPostResponseResource)Client.Post<TResource>(Uri, postAction, null);
        }


        public TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource
            where TSubResponseResource : TPostResponseResource
        {
            return Post<TSubResource, TSubResponseResource>(postAction, null);
        }


        public object Post<TPostForm>(TResource resource, TPostForm form)
            where TPostForm : class, IPostForm, IClientResource
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return Client.Post(((IHasResourceUri)resource).Uri, form, null);
        }


        public virtual async Task<TSubResponseResource> PostAsync<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                                                      Action
                                                                                                          <
                                                                                                          IRequestOptions
                                                                                                          <TSubResponseResource>> options)
            where TSubResource : class, TResource where TSubResponseResource : TPostResponseResource
        {
            return (TSubResponseResource)await Client.PostAsync(Uri, postAction, RequestOptions.Create(options));
        }


        public IQueryProvider Provider
        {
            get { return new RestQueryProvider(Client); }
        }


        public IQueryable<TSubResource> Query<TSubResource>()
            where TSubResource : TResource
        {
            return Client.Query<TSubResource>(Uri);
        }


        public IQueryable<TResource> Query()
        {
            return Client.Query<TResource>(Uri);
        }


        public string Uri { get; }


        void IDeletableByIdRepository<TId>.Delete(TId id)
        {
            Client.Delete(GetLazy(id), null);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}