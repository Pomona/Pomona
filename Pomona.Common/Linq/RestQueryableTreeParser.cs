using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Pomona.Common.Internals;
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
        private StringBuilder expandedPaths = new StringBuilder();
        private LambdaExpression groupByKeySelector;

        private LambdaExpression orderKeySelector;

        private QueryProjection projection = QueryProjection.Enumerable;
        private LambdaExpression selectExpression;

        private int? skipCount;
        private SortOrder sortOrder = SortOrder.Ascending;
        private int? takeCount;
        private IList<LambdaExpression> whereExpressions = new List<LambdaExpression>();
        private LambdaExpression wherePredicate;


        static RestQueryableTreeParser()
        {
            visitQueryConstantValueMethod =
                ReflectionHelper.GetGenericMethodDefinition<RestQueryableTreeParser>(
                    x => x.VisitQueryConstantValue<object>(null));
            MapQueryableFunction(QueryableMethods.Take);
            MapQueryableFunction(QueryableMethods.Skip);
            MapQueryableFunction(QueryableMethods.Where);
            MapQueryableFunction(QueryableMethods.OrderBy);
            MapQueryableFunction(QueryableMethods.OrderByDescending);
            MapQueryableFunction(QueryableMethods.First);
            MapQueryableFunction(QueryableMethods.FirstOrDefault);
            MapQueryableFunction(QueryableMethods.FirstWithPredicate);
            MapQueryableFunction(QueryableMethods.FirstOrDefaultWithPredicate);
            MapQueryableFunction(QueryableMethods.AnyWithPredicate);
            MapQueryableFunction(QueryableMethods.Select);
            MapQueryableFunction(QueryableMethods.GroupBy);
            MapQueryableFunction(QueryableMethods.Expand);
        }


        public Type ElementType
        {
            get { return this.elementType; }
        }

        public string ExpandedPaths
        {
            get { return this.expandedPaths.ToString(); }
        }

        public LambdaExpression GroupByKeySelector
        {
            get { return this.groupByKeySelector; }
        }

        public LambdaExpression OrderKeySelector
        {
            get { return this.orderKeySelector; }
        }

        public QueryProjection Projection
        {
            get { return this.projection; }
        }

        public LambdaExpression SelectExpression
        {
            get { return this.selectExpression; }
        }

        public Type SelectReturnType
        {
            get
            {
                if (this.selectExpression == null)
                    return this.elementType;
                return this.selectExpression.ReturnType;
            }
        }

        public int? SkipCount
        {
            get { return this.skipCount; }
        }

        public SortOrder SortOrder
        {
            get { return this.sortOrder; }
        }

        public int? TakeCount
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

            try
            {
                visitMethodInstance.Invoke(
                    this,
                    node.Arguments.Skip(1)
                        .Select(ExtractArgumentFromExpression)
                        .ToArray());
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException ?? targetInvocationException;
            }

            return node;
        }


        internal void QAny<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            // TODO: When count is supported it will work better..
            QWhere(predicate);
            this.takeCount = 1;
            this.projection = QueryProjection.Any;
        }


        internal void QExpand<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (this.expandedPaths.Length > 0)
                this.expandedPaths.Append(',');
            this.expandedPaths.Append(propertySelector.GetPropertyPath(true));
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
            this.projection = QueryProjection.FirstOrDefault;
        }


        internal void QFirstOrDefault<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QFirstOrDefault<TSource>();
        }


        internal void QGroupBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            if (SkipCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Skip() before GroupBy()");
            if (TakeCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Take() before GroupBy()");
            if (this.groupByKeySelector != null)
                throw new NotSupportedException("Pomona LINQ provider does not support multiple chained GroupBy()");
            this.groupByKeySelector = keySelector;
        }


        internal void QOrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            OrderBy(keySelector, SortOrder.Ascending);
        }


        internal void QOrderByDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            OrderBy(keySelector, SortOrder.Descending);
        }


        internal void QSelect<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            if (this.selectExpression != null)
            {
                throw new NotSupportedException(
                    "Pomona LINQ provider does not support calling Select() multiple times.");
            }
            if (this.expandedPaths.Length > 0)
                throw new NotSupportedException("Pomona LINQ provider does not support using Expand() before Select()");
            this.selectExpression = selector;
        }


        internal void QSkip<TSource>(int skipCount)
        {
            if (TakeCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Take() before Skip().");
            if (SkipCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support multiple calls to Skip()");
            this.skipCount = skipCount;
        }


        internal void QTake<TSource>(int takeCount)
        {
            if (TakeCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support multiple calls to Take()");
            this.takeCount = takeCount;
        }


        internal void QWhere<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            if (SelectExpression != null)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Where() after Select()");
            if (GroupByKeySelector != null)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Where() after GroupBy()");

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
            MapQueryableFunction(ReflectionHelper.GetGenericMethodDefinition(expr));
        }


        private static void MapQueryableFunction(MethodInfo method)
        {
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


        private void OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector, SortOrder sortOrder)
        {
            if (SkipCount.HasValue)
            {
                throw new NotSupportedException(
                    "Pomona LINQ provider does not support calling Skip() before OrderBy/OrderByDescending");
            }
            if (TakeCount.HasValue)
            {
                throw new NotSupportedException(
                    "Pomona LINQ provider does not support calling Take() before OrderBy/OrderByDescending");
            }
            this.orderKeySelector = keySelector;
            this.sortOrder = sortOrder;
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