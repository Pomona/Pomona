#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

namespace Pomona.Common.Linq.Queries.Interception
{
    public interface IInterceptedQueryableSource : IQueryable
    {
        IQueryable WrappedSource { get; }
    }
}

