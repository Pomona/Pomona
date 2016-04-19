#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public class ChildResourceRepository<TResource, TPostResponseResource, TId>
        : ClientRepository<TResource, TPostResponseResource, TId>
        where TResource : class, IClientResource
        where TPostResponseResource : IClientResource
    {
        private readonly IClientResource parent;


        public ChildResourceRepository(IPomonaClient client, string uri, IEnumerable results, IClientResource parent)
            : base(client, uri, results, parent)
        {
            this.parent = parent;
        }


        public override TPostResponseResource Post(IPostForm form)
        {
            var requestOptions = new RequestOptions();
            AddEtagOptions(requestOptions);
            return (TPostResponseResource)Client.Post(Uri, (TResource)((object)form), requestOptions);
        }


        public override TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
        {
            return base.Post(postAction, AddEtagOptions);
        }


        public override TPostResponseResource Post(Action<TResource> postAction)
        {
            return base.Post(postAction, AddEtagOptions);
        }


        public override TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                                      Action<IRequestOptions<TSubResponseResource>> options)
        {
            return base.Post<TSubResource, TSubResponseResource>(postAction,
                                                                 x =>
                                                                 {
                                                                     if (options != null)
                                                                         options(x);
                                                                     AddEtagOptions(x);
                                                                 });
        }


        public override TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction,
                                                                 Action<IRequestOptions<TPostResponseResource>> options)
        {
            return base.Post(postAction,
                             x =>
                             {
                                 if (options != null)
                                     options(x);
                                 AddEtagOptions(x);
                             });
        }


        private void AddEtagOptions(IRequestOptions options)
        {
            var parentResourceInfo = Client.GetMostInheritedResourceInterfaceInfo(this.parent.GetType());
            if (parentResourceInfo.HasEtagProperty)
            {
                var etag = parentResourceInfo.EtagProperty.GetValue(this.parent, null);
                options.ModifyRequest(r => r.Headers.Add("If-Match", $"\"{etag}\""));
            }
        }
    }
}