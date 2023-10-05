#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Pomona.Common.Linq.NonGeneric;

namespace Pomona.RequestProcessing
{
    internal interface IQueryableActionResult : IQueryable, IActionResult
    {
        int? DefaultPageSize { get; }
        QueryProjection Projection { get; }
        IQueryable WrappedQueryable { get; }
    }

    internal interface IQueryableActionResult<out TElement> : IQueryable<TElement>, IQueryableActionResult
    {
    }

    internal interface IQueryableActionResult<out TElement, TResult> : IQueryableActionResult<TElement>
    {
    }
}

