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
    public class ZipExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Zip(Enumerable.Empty<object>(), (a, b) => a));

        private static ZipFactory factory;
        private QueryExpression source2;


        private ZipExpression(MethodCallExpression node, QueryExpression source)
            : this(node, source, null)
        {
        }


        private ZipExpression(MethodCallExpression node, QueryExpression source, QueryExpression source2)
            : base(node, source)
        {
            this.source2 = source2;
            if (source2 != null && source2.Node != node.Arguments[1])
                throw new ArgumentException("Argument at index 1 of MethodCallExpression need to match source2.Node.");
        }


        public static QueryExpressionFactory Factory => factory ?? (factory = new ZipFactory());

        public LambdaExpression ResultSelector => (LambdaExpression)((UnaryExpression)Arguments[2]).Operand;

        public QueryExpression Source2 => this.source2 ?? (this.source2 = Wrap(Arguments[1]));


        public static ZipExpression Create(QueryExpression source,
                                           QueryExpression source2,
                                           LambdaExpression resultSelector)
        {
            return factory.Create(source, source2, resultSelector);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var origSource2 = Source2;
            var origResultSelector = ResultSelector;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitChildren");
            var visitedSource2 = visitor.VisitAndConvert(origSource2, "VisitChildren");
            var visitedResultSelector = visitor.VisitAndConvert(origResultSelector, "VisitChildren");
            if (visitedSource != origSource || visitedSource2 != origSource2
                || visitedResultSelector != origResultSelector)
                return Create(visitedSource, visitedSource2, origResultSelector);
            return this;
        }

        #region Nested type: ZipFactory

        private class ZipFactory : QueryChainedExpressionFactory<ZipExpression>
        {
            public ZipExpression Create(QueryExpression source, QueryExpression source2, LambdaExpression resultSelector)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                if (source2 == null)
                    throw new ArgumentNullException(nameof(source2));
                if (resultSelector == null)
                    throw new ArgumentNullException(nameof(resultSelector));
                return
                    new ZipExpression(
                        Call(
                            Method.MakeGenericMethod(source.ElementType, source2.ElementType, resultSelector.ReturnType),
                            source.Node,
                            source2.Node,
                            ConvertAndQuote(resultSelector, source.ElementType)),
                        source,
                        source2);
            }
        }

        #endregion
    }
}

