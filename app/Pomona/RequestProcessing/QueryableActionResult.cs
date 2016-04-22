#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

using Pomona.Common.Linq;
using Pomona.Common.Linq.NonGeneric;

namespace Pomona.RequestProcessing
{
    internal class QueryableActionResult<TElement, TResult>
        : WrappedQueryableBase<TElement>, IQueryableActionResult<TElement, TResult>
    {
        internal QueryableActionResult(IQueryable<TElement> innerQueryable,
                                       QueryProjection projection,
                                       int? defaultPageSize)
            : base(innerQueryable)
        {
            Projection = projection;
            DefaultPageSize = defaultPageSize;
        }


        public Type ResultType => typeof(TResult);

        public int? DefaultPageSize { get; private set; }

        public QueryProjection Projection { get; }

        public IQueryable WrappedQueryable => InnerQueryable;
    }
}