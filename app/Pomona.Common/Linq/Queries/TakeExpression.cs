#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries
{
    public class TakeExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Take(0));

        private static TakeFactory factory;


        private TakeExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public int Count => (int)((ConstantExpression)Arguments[1]).Value;

        public static QueryExpressionFactory Factory => factory ?? (factory = new TakeFactory());


        public static TakeExpression Create(QueryExpression source, int count)
        {
            return factory.Create(source, count);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitChildren");
            if (visitedSource != origSource)
                return Create(visitedSource, Count);
            return this;
        }

        #region Nested type: TakeFactory

        private class TakeFactory : QueryChainedExpressionFactory<TakeExpression>
        {
            public TakeExpression Create(QueryExpression source, int count)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                return new TakeExpression(Call(Method.MakeGenericMethod(source.ElementType),
                                               source.Node,
                                               Constant(count)),
                                          source);
            }
        }

        #endregion
    }
}

