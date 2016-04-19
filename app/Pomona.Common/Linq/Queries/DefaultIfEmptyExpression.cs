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
    public class DefaultIfEmptyExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.DefaultIfEmpty());

        private static DefaultIfEmptyFactory factory;


        private DefaultIfEmptyExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory
        {
            get { return factory ?? (factory = new DefaultIfEmptyFactory()); }
        }


        public static DefaultIfEmptyExpression Create(QueryExpression source)
        {
            return factory.Create(source);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Console.WriteLine("DefaultIfEmpty");
            QueryExpression origSource = Source;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitDefaultIfEmpty");
            if (visitedSource != origSource)
                return Create(visitedSource);
            return this;
        }

        #region Nested type: DefaultIfEmptyFactory

        private class DefaultIfEmptyFactory : QueryChainedExpressionFactory<DefaultIfEmptyExpression>
        {
            public DefaultIfEmptyExpression Create(QueryExpression source)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                return new DefaultIfEmptyExpression(Call(Method.MakeGenericMethod(source.ElementType), source.Node),
                                                    source);
            }
        }

        #endregion
    }
}