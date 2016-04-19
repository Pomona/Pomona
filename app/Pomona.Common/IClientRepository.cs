#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public interface IClientRepository
    {
        string Uri { get; }
    }

    public interface IClientRepository<TResource, TPostResponseResource, TId>
        : IQueryableRepository<TResource>,
            IPatchableRepository<TResource>,
            IPostableRepository<TResource, TPostResponseResource>,
            IDeletableRepository<TResource>
        where TResource : class, IClientResource
        where TPostResponseResource : IClientResource
    {
        TPostResponseResource Post(IPostForm form);
    }
}