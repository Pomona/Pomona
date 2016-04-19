#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Threading.Tasks;

namespace Pomona.Common
{
    public interface IDeletableRepository<TResource>
        where TResource : class, IClientResource
    {
        void Delete(TResource resource);

        Task DeleteAsync(TResource resource);
    }
}