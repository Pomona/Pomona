#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    internal class QueryOrderByBuilder : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node.NodeType != ExpressionType.Constant
                || !(typeof(IEnumerable<Tuple<LambdaExpression, SortOrder>>).IsAssignableFrom(node.Type)))
                throw new NotSupportedException("This visitor only supports IEnumerable<Tuple<LambdaExpression, SortOrder>> constant.");

            return base.Visit(node);
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            return Visit((IEnumerable<Tuple<LambdaExpression, SortOrder>>)node.Value);
        }


        private PomonaExtendedExpression Visit(IEnumerable<Tuple<LambdaExpression, SortOrder>> orderKeySelectors)
        {
            return
                new QueryOrderExpression(
                    orderKeySelectors.Select(
                        x =>
                            new Tuple<PomonaExtendedExpression, SortOrder>(
                            (PomonaExtendedExpression)ExpressionExtensions.Visit<QueryPredicateBuilder>(x.Item1), x.Item2)), typeof(object));
        }
    }
}