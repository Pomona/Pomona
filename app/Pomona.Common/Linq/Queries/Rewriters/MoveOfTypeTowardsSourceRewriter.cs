#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq.Expressions;

using Pomona.Common.Expressions;

namespace Pomona.Common.Linq.Queries.Rewriters
{
    public class MoveOfTypeTowardsSourceRewriter : QueryExpressionRewriter<OfTypeExpression>
    {
        public override Expression Visit(IRewriteContext context, OfTypeExpression node)
        {
            if (node.ElementType == node.Source.ElementType)
                return node.Source;

            var isExpandingTypeOf = node.Source.ElementType.IsAssignableFrom(node.ElementType);
            var whereNode = node.Source as WhereExpression;
            if (isExpandingTypeOf && whereNode != null)
                return whereNode.Source.OfType(node.ElementType).Where(whereNode.Predicate);
            return node;
        }
    }
}
