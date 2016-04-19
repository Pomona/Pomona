#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public abstract class QueryChainedExpression : QueryMethodExpression
    {
        protected QueryChainedExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }
    }
}