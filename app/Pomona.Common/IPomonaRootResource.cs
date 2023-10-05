#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

namespace Pomona.Common
{
    public interface IPomonaRootResource : IPomonaClient
    {
        IQueryable<T> Query<T>();
    }
}

