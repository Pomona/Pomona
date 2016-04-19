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
    public class SelectExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Select(y => false));

        private static SelectFactory factory;


        private SelectExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory => factory ?? (factory = new SelectFactory());

        public LambdaExpression Selector => (LambdaExpression)((UnaryExpression)Arguments[1]).Operand;


        public static SelectExpression Create(QueryExpression source, LambdaExpression selector)
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
                return factory.Create(visitedSource, visitedSelector);
            return this;
        }

        #region Nested type: WhereFactory

        internal class SelectFactory : QueryChainedExpressionFactory<SelectExpression>
        {
            public SelectExpression Create(QueryExpression source, LambdaExpression selector)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                if (selector == null)
                    throw new ArgumentNullException(nameof(selector));
                return new SelectExpression(Call(Method.MakeGenericMethod(source.ElementType, selector.ReturnType),
                                                 source.Node,
                                                 ConvertAndQuote(selector, source.ElementType)),
                                            source);
            }
        }

        #endregion
    }
}