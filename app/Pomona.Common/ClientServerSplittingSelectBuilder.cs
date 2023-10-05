#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq.Expressions;

using Pomona.Common.Linq;

namespace Pomona.Common
{
    internal class ClientServerSplittingSelectBuilder : QuerySelectorBuilder
    {
        protected override Expression VisitRootLambda<T>(Expression<T> node)
        {
            var visited = base.VisitRootLambda(node);
            var pomonaExpr = visited as PomonaExtendedExpression;
            if (pomonaExpr != null && !pomonaExpr.SupportedOnServer)
                return new SelectClientServerPartitionerVisitor(this).SplitExpression(node);
            return visited;
        }
    }
}
