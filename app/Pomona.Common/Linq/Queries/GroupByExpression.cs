#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries
{
    public class GroupByExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.GroupBy(y => false));

        private static GroupByFactory factory;


        private GroupByExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory => factory ?? (factory = new GroupByFactory());

        public LambdaExpression KeySelector => (LambdaExpression)((UnaryExpression)Arguments[1]).Operand;


        public static GroupByExpression Create(QueryExpression source, LambdaExpression keySelector)
        {
            return factory.Create(source, keySelector);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var origKeySelector = KeySelector;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitChildren");
            var visitedKeySelector = visitor.VisitAndConvert(origKeySelector, "VisitChildren");
            if (visitedSource != origSource || visitedKeySelector != origKeySelector)
                return Create(visitedSource, visitedKeySelector);
            return this;
        }

        #region Nested type: GroupByFactory

        private class GroupByFactory : QueryChainedExpressionFactory<GroupByExpression>
        {
            public GroupByExpression Create(QueryExpression source, LambdaExpression keySelector)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                if (keySelector == null)
                    throw new ArgumentNullException(nameof(keySelector));
                return new GroupByExpression(Call(Method.MakeGenericMethod(source.ElementType, keySelector.ReturnType),
                                                  source.Node,
                                                  ConvertAndQuote(keySelector, source.ElementType)),
                                             source);
            }
        }

        #endregion
    }
}