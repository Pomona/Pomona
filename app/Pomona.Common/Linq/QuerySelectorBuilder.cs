#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal class QuerySelectorBuilder : QueryPredicateBuilder
    {
        protected override Expression VisitRootLambda<T>(Expression<T> node)
        {
            if (node.Body.NodeType == ExpressionType.New)
                return VisitRootNew((NewExpression)node.Body);
            if (node.Body.NodeType == ExpressionType.ListInit)
                return VisitRootListInit((ListInitExpression)node.Body);
            return Nodes(node, base.VisitRootLambda(node), " as this");
        }


        private Expression VisitRootListInit(ListInitExpression body)
        {
            return Visit(body);
        }


        private Expression VisitRootNew(NewExpression node)
        {
            if (!(node.Constructor.DeclaringType.IsAnonymous() || node.Constructor.DeclaringType.IsTuple()))
                return Visit(node);

            var readOnlyCollection = node.Members != null
                ? node.Members.Select(x => x.Name)
                : node.Arguments.Select((x, i) => string.Format("Item{0}", i + 1));

            var selectList = node.Arguments.Zip(
                readOnlyCollection,
                (e, p) => new KeyValuePair<string, PomonaExtendedExpression>(p, (PomonaExtendedExpression)Visit(e)))
                                 .ToList();
            return new QuerySelectExpression(selectList, node.Type);
        }
    }
}