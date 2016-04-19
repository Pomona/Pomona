#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries
{
    public class SelectManyExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.SelectMany(y => new object[] { }));

        private static SelectManyFactory factory;


        private SelectManyExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory
        {
            get { return factory ?? (factory = new SelectManyFactory()); }
        }

        public LambdaExpression Selector
        {
            get { return (LambdaExpression)((UnaryExpression)Arguments[1]).Operand; }
        }


        public static SelectManyExpression Create(QueryExpression source, LambdaExpression selector)
        {
            return factory.Create(source, selector);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var origSelector = Selector;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitSelect");
            var visitedSelector = visitor.VisitAndConvert(origSelector, "VisitSelect");
            if (visitedSource != origSource || visitedSelector != origSelector)
                return Create(visitedSource, visitedSelector);
            return this;
        }

        #region Nested type: SelectManyFactory

        private class SelectManyFactory : QueryChainedExpressionFactory<SelectManyExpression>
        {
            public SelectManyExpression Create(QueryExpression source, LambdaExpression selector)
            {
                return Create(Call(
                    Method.MakeGenericMethod(source.ElementType, selector.ReturnType.GetGenericArguments()[0]),
                    source.Node,
                    ConvertAndQuote(selector, source.ElementType)),
                              source);
            }
        }

        #endregion
    }
}