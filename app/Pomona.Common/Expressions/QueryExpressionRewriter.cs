#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

using Pomona.Common.Linq.Queries;

namespace Pomona.Common.Expressions
{
    public abstract class QueryExpressionRewriter<TExpression> : ExpressionRewriter<TExpression>
        where TExpression : QueryChainedExpression
    {
        private static readonly IEnumerable<Type> visitedTypes =
            new ReadOnlyCollection<Type>(new[] { typeof(TExpression), typeof(MethodCallExpression) });

        public override IEnumerable<Type> VisitedTypes
        {
            get { return visitedTypes; }
        }


        internal override Expression OnVisit(IRewriteContext context, Expression node)
        {
            if (node == null)
                return null;

            TExpression queryExpression = node as TExpression;
            if (queryExpression != null)
                return Visit(context, queryExpression);

            if (!(node is QueryExpression) && QueryExpression.TryWrap(node, out queryExpression))
            {
                var visited = Visit(context, queryExpression);
                var visitedAsQueryExpression = visited as QueryExpression;
                if (visitedAsQueryExpression != null)
                {
                    // Unwrap
                    return visitedAsQueryExpression.Node;
                }
                return visited;
            }
            return node;
        }
    }
}