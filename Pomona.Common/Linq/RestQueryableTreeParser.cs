using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Internals;

namespace Pomona.Common.Linq
{
    internal class RestQueryableTreeParser : ExpressionVisitor
    {
        #region QueryProjection enum

        public enum QueryProjection
        {
            Enumerable,
            First,
            FirstOrDefault,
            Any
        }

        #endregion

        private static readonly Dictionary<int, MethodInfo> queryableMethodToVisitMethodDictionary =
            new Dictionary<int, MethodInfo>();

        private static readonly MethodInfo visitQueryConstantValueMethod;
        private Type elementType;

        private LambdaExpression orderKeySelector;

        private QueryProjection projection = QueryProjection.Enumerable;

        private int skipCount = 0;
        private SortOrder sortOrder = SortOrder.Ascending;
        private int takeCount = int.MaxValue;
        private IList<LambdaExpression> whereExpressions = new List<LambdaExpression>();
        private LambdaExpression wherePredicate;


        static RestQueryableTreeParser()
        {
            visitQueryConstantValueMethod =
                ReflectionHelper.GetGenericMethodDefinition<RestQueryableTreeParser>(
                    x => x.VisitQueryConstantValue<object>(null));
            MapQueryableFunction(x => x.Take(0));
            MapQueryableFunction(x => x.Skip(0));
            MapQueryableFunction(x => x.Where(y => false));
            MapQueryableFunction(x => x.OrderBy(y => y));
            MapQueryableFunction(x => x.OrderByDescending(y => y));
            MapQueryableFunction(x => x.First());
            MapQueryableFunction(x => x.FirstOrDefault());
            MapQueryableFunction(x => x.First(y => false));
            MapQueryableFunction(x => x.FirstOrDefault(y => false));
            MapQueryableFunction(x => x.Any(null));
            MapQueryableFunction(x => x.Select(y => 0));
        }


        public Type ElementType
        {
            get { return this.elementType; }
        }

        public LambdaExpression OrderKeySelector
        {
            get { return this.orderKeySelector; }
        }

        public QueryProjection Projection
        {
            get { return this.projection; }
        }

        public int SkipCount
        {
            get { return this.skipCount; }
        }

        public SortOrder SortOrder
        {
            get { return this.sortOrder; }
        }

        public int TakeCount
        {
            get { return this.takeCount; }
        }

        public LambdaExpression WherePredicate
        {
            get { return this.wherePredicate; }
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Using chained (extension method) calling style this will be the source of the query.
            // source.Where(...) etc..
            if (node.Type.MetadataToken == typeof(RestQuery<>).MetadataToken)
            {
                visitQueryConstantValueMethod.MakeGenericMethod(node.Type.GetGenericArguments()).Invoke(
                    this, new[] { node.Value });
                return node;
            }

            throw new NotImplementedException(
                "Don't know what to do with constant node of type " + node.Type.FullName + " here..");
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Visit(node.Arguments[0]);
            var visitMethod = queryableMethodToVisitMethodDictionary[node.Method.MetadataToken];
            var visitMethodInstance = visitMethod.MakeGenericMethod(node.Method.GetGenericArguments());
            visitMethodInstance.Invoke(
                this,
                node.Arguments.Skip(1)
                    .Select(ExtractArgumentFromExpression)
                    .ToArray());

            return node;
        }


        internal void QAny<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            // TODO: When count is supported it will work better..
            QWhere(predicate);
            this.takeCount = 1;
            this.projection = QueryProjection.Any;
        }


        internal void QFirst<TSource>()
        {
            this.takeCount = 1;
            this.projection = QueryProjection.First;
        }


        internal void QFirst<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QFirst<TSource>();
        }


        internal void QFirstOrDefault<TSource>()
        {
            this.takeCount = 1;
            this.projection = QueryProjection.First;
        }


        internal void QFirstOrDefault<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QFirstOrDefault<TSource>();
        }


        internal void QOrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            this.orderKeySelector = keySelector;
            this.sortOrder = SortOrder.Ascending;
        }


        internal void QOrderByDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            this.orderKeySelector = keySelector;
            this.sortOrder = SortOrder.Descending;
        }


        internal void QSelect<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            throw new NotImplementedException();
        }


        internal void QSkip<TSource>(int skipCount)
        {
            this.skipCount = skipCount;
        }


        internal void QTake<TSource>(int takeCount)
        {
            this.takeCount = takeCount;
        }


        internal void QWhere<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            this.whereExpressions.Add(predicate);
            if (this.wherePredicate == null)
                this.wherePredicate = predicate;
            else
            {
                var replacer = new LamdbaParameterReplacer(predicate.Parameters[0], this.wherePredicate.Parameters[0]);
                var rewrittenPredicateBody = replacer.Visit(predicate.Body);
                this.wherePredicate = Expression.Lambda(
                    this.wherePredicate.Type,
                    Expression.AndAlso(this.wherePredicate.Body, rewrittenPredicateBody),
                    this.wherePredicate.Parameters);
            }
        }


        private static void MapQueryableFunction(Expression<Func<IQueryable<int>, object>> expr)
        {
            var method = ReflectionHelper.GetGenericMethodDefinition(expr);
            var visitMethod = typeof(RestQueryableTreeParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => VisitMethodMatches(x, method));
            queryableMethodToVisitMethodDictionary.Add(method.MetadataToken, visitMethod);
        }


        private static bool VisitMethodMatches(MethodInfo visitorMethod, MethodInfo nodeMethod)
        {
            var visitMethodName = "Q" + nodeMethod.Name;
            if (visitorMethod.Name != visitMethodName)
                return false;

            var visitorMethodParams = visitorMethod.GetParameters();
            var nodeMethodParams = nodeMethod.GetParameters();

            if (visitorMethodParams.Length != nodeMethodParams.Length - 1)
                return false;

            return visitorMethodParams
                .Zip(nodeMethodParams.Skip(1), (x, y) => x.ParameterType.IsGenericallyEquivalentTo(y.ParameterType))
                .All(x => x);
        }


        private object ExtractArgumentFromExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Quote)
                return ((UnaryExpression)expression).Operand;
            if (expression.NodeType == ExpressionType.Lambda)
                return expression;
            if (expression.NodeType == ExpressionType.Constant)
                return ((ConstantExpression)expression).Value;
            throw new NotSupportedException("Does not know how to unwrap " + expression.NodeType);
        }


        private object VisitQueryConstantValue<T>(RestQuery<T> restQuery)
        {
            this.elementType = ((IQueryable)restQuery).ElementType;
            return null;
        }

        #region Nested type: LamdbaParameterReplacer

        private class LamdbaParameterReplacer : ExpressionVisitor
        {
            private ParameterExpression replaceParam;
            private ParameterExpression searchParam;


            public LamdbaParameterReplacer(ParameterExpression searchParam, ParameterExpression replaceParam)
            {
                this.searchParam = searchParam;
                this.replaceParam = replaceParam;
            }


            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == this.searchParam)
                    return this.replaceParam;
                return base.VisitParameter(node);
            }
        }

        #endregion
    }
}