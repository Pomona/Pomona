#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public abstract class QueryExpressionFactory
    {
        public abstract bool TryWrapNode(Expression node, out QueryExpression wrapper);


        public QueryExpression WrapNode(Expression node)
        {
            QueryExpression wrapper;
            if (!TryWrapNode(node, out wrapper))
                throw new ArgumentException("Expression not wrappable by factory", nameof(node));
            return wrapper;
        }


        internal QueryExpression WrapOrNull(Expression node)
        {
            QueryExpression wrapper;
            if (!TryWrapNode(node, out wrapper))
                return null;
            return wrapper;
        }
    }
}