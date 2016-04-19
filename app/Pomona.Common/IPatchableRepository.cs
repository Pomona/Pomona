#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Common
{
    public interface IPatchableRepository<TResource>
        where TResource : class, IClientResource
    {
        TSubResource Patch<TSubResource>(TSubResource resource,
                                         Action<TSubResource> patchAction,
                                         Action<IRequestOptions<TSubResource>> options)
            where TSubResource : class, TResource;


        TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction)
            where TSubResource : class, TResource;


        Task<TSubResource> PatchAsync<TSubResource>(TSubResource resource,
                                                    Action<TSubResource> patchAction,
                                                    Action<IRequestOptions<TSubResource>> options = null)
            where TSubResource : class, TResource;
    }
}