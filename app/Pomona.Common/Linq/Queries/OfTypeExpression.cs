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
    public class OfTypeExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.OfType<object>());

        private static OfTypeFactory factory;


        private OfTypeExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory => factory ?? (factory = new OfTypeFactory());


        public static OfTypeExpression Create(QueryExpression source, Type type)
        {
            return factory.Create(source, type);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Console.WriteLine("OfType");
            QueryExpression origSource = Source;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitOfType");
            if (visitedSource != origSource)
                return Create(visitedSource, ElementType);
            return this;
        }

        #region Nested type: OfTypeFactory

        private class OfTypeFactory : QueryChainedExpressionFactory<OfTypeExpression>
        {
            public OfTypeExpression Create(QueryExpression source, Type type)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                return new OfTypeExpression(Call(Method.MakeGenericMethod(type), source.Node), source);
            }
        }

        #endregion
    }
}