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
    public class WhereExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Where(y => false));

        private static WhereFactory factory;


        private WhereExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory
        {
            get { return factory ?? (factory = new WhereFactory()); }
        }

        public LambdaExpression Predicate
        {
            get { return (LambdaExpression)((UnaryExpression)Arguments[1]).Operand; }
        }


        public static WhereExpression Create(QueryExpression source, LambdaExpression predicate)
        {
            return factory.Create(source, predicate);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var origPredicate = Predicate;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitChildren");
            var visitedPredicate = visitor.VisitAndConvert(origPredicate, "VisitChildren");
            if (visitedSource != origSource || visitedPredicate != origPredicate)
                return Create(visitedSource, visitedPredicate);
            return this;
        }

        #region Nested type: WhereFactory

        private class WhereFactory : QueryChainedExpressionFactory<WhereExpression>
        {
            public WhereExpression Create(QueryExpression source, LambdaExpression predicate)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                if (predicate == null)
                    throw new ArgumentNullException(nameof(predicate));
                return new WhereExpression(Call(Method.MakeGenericMethod(source.ElementType),
                                                source.Node,
                                                ConvertAndQuote(predicate, source.ElementType)),
                                           source);
            }
        }

        #endregion
    }
}