#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Common
{
    public static class ClientRepositoryExtensions
    {
        public static Task<TPostResponseResource> PostAsync<TResource, TPostResponseResource>(
            this IPostableRepository<TResource, TPostResponseResource> repository,
            Action<TResource> action)
            where TResource : class, IClientResource
            where TPostResponseResource : IClientResource
        {
            return repository.PostAsync<TResource, TPostResponseResource>(action, null);
        }
    }
}