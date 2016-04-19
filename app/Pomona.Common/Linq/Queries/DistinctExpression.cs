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
    public class DistinctExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Distinct());

        private static DistinctFactory factory;


        private DistinctExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory => factory ?? (factory = new DistinctFactory());


        public static DistinctExpression Create(QueryExpression source)
        {
            return factory.Create(source);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Console.WriteLine("Distinct");
            QueryExpression origSource = Source;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitDistinct");
            if (visitedSource != origSource)
                return Create(visitedSource);
            return this;
        }

        #region Nested type: DistinctFactory

        private class DistinctFactory : QueryChainedExpressionFactory<DistinctExpression>
        {
            public DistinctExpression Create(QueryExpression source)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                return new DistinctExpression(Call(Method.MakeGenericMethod(source.ElementType), source.Node),
                                              source);
            }
        }

        #endregion
    }
}