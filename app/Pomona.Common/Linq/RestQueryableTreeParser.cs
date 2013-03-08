// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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

        private static readonly Dictionary<long, MethodInfo> queryableMethodToVisitMethodDictionary =
            new Dictionary<long, MethodInfo>();

        private static readonly MethodInfo visitQueryConstantValueMethod;
        private readonly StringBuilder expandedPaths = new StringBuilder();
        private readonly IList<LambdaExpression> whereExpressions = new List<LambdaExpression>();
        private Type elementType;
        private LambdaExpression groupByKeySelector;
        private bool includeTotalCount;

        private LambdaExpression orderKeySelector;

        private QueryProjection projection = QueryProjection.Enumerable;
        private LambdaExpression selectExpression;

        private int? skipCount;
        private SortOrder sortOrder = SortOrder.Ascending;
        private int? takeCount;
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
            MapQueryableFunction(QueryableMethods.SumIntWithSelector);
            MapQueryableFunction(QueryableMethods.IncludeTotalCount);
        }

        public bool IncludeTotalCount
        {
            get { return includeTotalCount; }
        }


        public Type ElementType
        {
            get { return elementType; }
        }

        public string ExpandedPaths
        {
            get { return expandedPaths.ToString(); }
        }

        public LambdaExpression GroupByKeySelector
        {
            get { return groupByKeySelector; }
        }

        public LambdaExpression OrderKeySelector
        {
            get { return orderKeySelector; }
        }

        public QueryProjection Projection
        {
            get { return projection; }
        }

        public LambdaExpression SelectExpression
        {
            get { return selectExpression; }
        }

        public Type SelectReturnType
        {
            get
            {
                if (selectExpression == null)
                    return elementType;
                return selectExpression.ReturnType;
            }
        }

        public int? SkipCount
        {
            get { return skipCount; }
        }

        public SortOrder SortOrder
        {
            get { return sortOrder; }
        }

        public int? TakeCount
        {
            get { return takeCount; }
        }

        public LambdaExpression WherePredicate
        {
            get { return wherePredicate; }
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Using chained (extension method) calling style this will be the source of the query.
            // source.Where(...) etc..
            if (node.Type.UniqueToken() == typeof (RestQuery<>).UniqueToken())
            {
                visitQueryConstantValueMethod.MakeGenericMethod(node.Type.GetGenericArguments()).Invoke(
                    this, new[] {node.Value});
                return node;
            }

            throw new NotImplementedException(
                "Don't know what to do with constant node of type " + node.Type.FullName + " here..");
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Visit(node.Arguments[0]);
            var visitMethod = queryableMethodToVisitMethodDictionary[node.Method.UniqueToken()];
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
            takeCount = 1;
            projection = QueryProjection.Any;
        }

        internal void QSum<TSource>(Expression<Func<TSource, int>> propertySelector)
        {
            throw new NotImplementedException();
        }

        internal void QExpand<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (expandedPaths.Length > 0)
                expandedPaths.Append(',');
            expandedPaths.Append(propertySelector.GetPropertyPath(true));
        }


        internal void QFirst<TSource>()
        {
            takeCount = 1;
            projection = QueryProjection.First;
        }


        internal void QFirst<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QFirst<TSource>();
        }

        internal void QIncludeTotalCount<TSource>()
        {
            includeTotalCount = true;
        }

        internal void QFirstOrDefault<TSource>()
        {
            takeCount = 1;
            projection = QueryProjection.FirstOrDefault;
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
            if (groupByKeySelector != null)
                throw new NotSupportedException("Pomona LINQ provider does not support multiple chained GroupBy()");
            groupByKeySelector = keySelector;
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
            if (selectExpression != null)
            {
                throw new NotSupportedException(
                    "Pomona LINQ provider does not support calling Select() multiple times.");
            }
            if (expandedPaths.Length > 0)
                throw new NotSupportedException("Pomona LINQ provider does not support using Expand() before Select()");
            selectExpression = selector;
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

            whereExpressions.Add(predicate);
            if (wherePredicate == null)
                wherePredicate = predicate;
            else
            {
                var replacer = new LamdbaParameterReplacer(predicate.Parameters[0], wherePredicate.Parameters[0]);
                var rewrittenPredicateBody = replacer.Visit(predicate.Body);
                wherePredicate = Expression.Lambda(
                    wherePredicate.Type,
                    Expression.AndAlso(wherePredicate.Body, rewrittenPredicateBody),
                    wherePredicate.Parameters);
            }
        }


        private static void MapQueryableFunction(Expression<Func<IQueryable<int>, object>> expr)
        {
            MapQueryableFunction(ReflectionHelper.GetGenericMethodDefinition(expr));
        }


        private static void MapQueryableFunction(MethodInfo method)
        {
            var visitMethod = typeof (RestQueryableTreeParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => VisitMethodMatches(x, method));

            if (visitMethod == null)
                throw new InvalidOperationException("Unable to find visitmethod to handle " + method.Name);

            queryableMethodToVisitMethodDictionary.Add(method.UniqueToken(), visitMethod);
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
                return ((UnaryExpression) expression).Operand;
            if (expression.NodeType == ExpressionType.Lambda)
                return expression;
            if (expression.NodeType == ExpressionType.Constant)
                return ((ConstantExpression) expression).Value;
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
            orderKeySelector = keySelector;
            this.sortOrder = sortOrder;
        }


        private object VisitQueryConstantValue<T>(RestQuery<T> restQuery)
        {
            elementType = ((IQueryable) restQuery).ElementType;
            return null;
        }

        #region Nested type: LamdbaParameterReplacer

        private class LamdbaParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression replaceParam;
            private readonly ParameterExpression searchParam;


            public LamdbaParameterReplacer(ParameterExpression searchParam, ParameterExpression replaceParam)
            {
                this.searchParam = searchParam;
                this.replaceParam = replaceParam;
            }


            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == searchParam)
                    return replaceParam;
                return base.VisitParameter(node);
            }
        }

        #endregion
    }
}