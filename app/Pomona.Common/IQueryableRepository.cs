#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public interface IQueryableRepository<TResource> : IClientRepository, IQueryable<TResource>
        where TResource : class, IClientResource
    {
        object Post<TPostForm>(TResource resource, TPostForm form)
            where TPostForm : class, IPostForm, IClientResource;


        IQueryable<TResource> Query();


        IQueryable<TSubResource> Query<TSubResource>()
            where TSubResource : TResource;
    }
}

