#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public class QueryableVisitor
        : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            QueryExpression qn;
            if (QueryExpression.TryWrap(node, out qn))
                return Visit(qn).ReduceExtensions();
            var visited = base.Visit(node);
            if (visited != node)
                return Visit(visited);
            return visited;
        }


        protected override Expression VisitExtension(Expression node)
        {
            var ofTypeExpression = node as OfTypeExpression;
            if (ofTypeExpression != null)
                return VisitOfType(ofTypeExpression);
            var whereExpression = node as WhereExpression;
            if (whereExpression != null)
                return VisitWhere(whereExpression);
            var selectManyExpression = node as SelectManyExpression;
            if (selectManyExpression != null)
                return VisitSelectMany(selectManyExpression);
            var selectExpression = node as SelectExpression;
            if (selectExpression != null)
                return VisitSelect(selectExpression);
            var querySourceExpression = node as QuerySourceExpression;
            if (querySourceExpression != null)
                return VisitQuerySource(querySourceExpression);
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitOfType(OfTypeExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitQuerySource(QuerySourceExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitSelect(SelectExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitSelectMany(SelectManyExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitWhere(WhereExpression node)
        {
            return base.VisitExtension(node);
        }
    }
}

