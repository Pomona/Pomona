#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common
{
    public interface IGettableRepository<TResource, TId> : IClientRepository
        where TResource : class, IClientResource
    {
        TResource Get(TId id);
        TResource GetLazy(TId id);
    }
}