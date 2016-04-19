#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq.Expressions;

using Pomona.Common.Expressions;
using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries.Rewriters
{
    public class MergeWhereRewriter : QueryExpressionRewriter<WhereExpression>
    {
        public override Expression Visit(IRewriteContext context, WhereExpression node)
        {
            var sourceAsWhere = node.Source as WhereExpression;
            if (sourceAsWhere != null)
            {
                var mergedPredicate = sourceAsWhere.Predicate.MergePredicateWith(node.Predicate);
                return WhereExpression.Create(sourceAsWhere.Source, mergedPredicate);
            }
            return node;
        }
    }
}