using System;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public class QuerySourceExpression : QueryExpression
    {
        private static QuerySourceFactory factory;
        public static QueryExpressionFactory Factory { get { return factory ?? (factory = new QuerySourceFactory()); } }

        private class QuerySourceFactory : QueryExpressionFactory
        {
            public override bool TryWrapNode(Expression node, out QueryExpression wrapper)
            {
                var nodeAsConstantExpression = node as ConstantExpression;
                Type[] typeArgs;
                if (nodeAsConstantExpression != null && nodeAsConstantExpression.Type.TryExtractTypeArguments(typeof(IQueryable<>), out typeArgs))
                {
                    wrapper = new QuerySourceExpression(nodeAsConstantExpression);
                    return true;
                }
                wrapper = null;
                return false;
            }
        }

        private QuerySourceExpression(ConstantExpression node)
            : base(node)
        {
        }
    }
}