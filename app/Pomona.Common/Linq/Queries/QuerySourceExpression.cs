#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public class QuerySourceExpression : QueryExpression
    {
        private static QuerySourceFactory factory;


        private QuerySourceExpression(ConstantExpression node)
            : base(node)
        {
        }


        public static QueryExpressionFactory Factory => factory ?? (factory = new QuerySourceFactory());

        private class QuerySourceFactory : QueryExpressionFactory
        {
            public override bool TryWrapNode(Expression node, out QueryExpression wrapper)
            {
                var nodeAsConstantExpression = node as ConstantExpression;
                Type[] typeArgs;
                if (nodeAsConstantExpression != null
                    && nodeAsConstantExpression.Type.TryExtractTypeArguments(typeof(IQueryable<>), out typeArgs))
                {
                    wrapper = new QuerySourceExpression(nodeAsConstantExpression);
                    return true;
                }
                wrapper = null;
                return false;
            }
        }
    }
}