#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Common
{
    public interface IPostableRepository<TResource, TPostResponseResource>
        where TResource : class, IClientResource
        where TPostResponseResource : IClientResource
    {
        TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource;


        TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction,
                                                 Action<IRequestOptions<TPostResponseResource>> options)
            where TSubResource : class, TResource;


        TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                      Action<IRequestOptions<TSubResponseResource>> options)
            where TSubResource : class, TResource
            where TSubResponseResource : TPostResponseResource;


        TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource
            where TSubResponseResource : TPostResponseResource;


        TPostResponseResource Post(Action<TResource> postAction);


        Task<TSubResponseResource> PostAsync<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                                 Action<IRequestOptions<TSubResponseResource>> options)
            where TSubResource : class, TResource
            where TSubResponseResource : TPostResponseResource;
    }
}
