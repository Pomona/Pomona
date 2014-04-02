#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    internal class RestQueryableTreeParser : ExpressionVisitor
    {
        #region QueryProjection enum

        public enum QueryProjection
        {
            Enumerable,
            First,
            FirstLazy,
            FirstOrDefault,
            Any,
            ToUri,
            Max,
            Min,
            Sum,
            ToJson,
            Count
        }

        #endregion

        private static readonly Dictionary<UniqueMemberToken, MethodInfo> queryableMethodToVisitMethodDictionary =
            new Dictionary<UniqueMemberToken, MethodInfo>();

        private readonly StringBuilder expandedPaths = new StringBuilder();
        private readonly IList<LambdaExpression> whereExpressions = new List<LambdaExpression>();
        private Type aggregateReturnType;
        private LambdaExpression groupByKeySelector;
        private bool includeTotalCount;
        private IRestQueryRoot queryRoot;
        private Type elementType;

        private readonly List<Tuple<LambdaExpression, SortOrder>> orderKeySelectors = new List<Tuple<LambdaExpression, SortOrder>>();

        private QueryProjection projection = QueryProjection.Enumerable;
        private LambdaExpression selectExpression;

        private int? skipCount;
        private int? takeCount;
        private LambdaExpression wherePredicate;


        static RestQueryableTreeParser()
        {
            foreach (var method in typeof (Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                TryMapQueryableFunction(method);
            }

            MapQueryableFunction(x => x.Expand(y => 0));
            MapQueryableFunction(x => x.IncludeTotalCount());
            MapQueryableFunction(x => x.ToUri());
            MapQueryableFunction(x => x.FirstLazy());
            MapQueryableFunction(x => x.ToJson());
        }

        public bool IncludeTotalCount
        {
            get { return includeTotalCount; }
        }

        public string RepositoryUri
        {
            get { return queryRoot.Uri; }
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

        public List<Tuple<LambdaExpression, SortOrder>> OrderKeySelectors
        {
            get { return orderKeySelectors; }
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
                if (aggregateReturnType != null)
                    return aggregateReturnType;

                if (selectExpression == null)
                    return ElementType;
                return selectExpression.ReturnType;
            }
        }

        public int? SkipCount
        {
            get { return skipCount; }
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
            var restQueryRoot = node.Value as IRestQueryRoot;
            if (restQueryRoot != null)
            {
                this.queryRoot = restQueryRoot;
                this.elementType = restQueryRoot.ElementType;
                return node;
            }

            throw new NotImplementedException(
                "Don't know what to do with constant node of type " + node.Type.FullName + " here..");
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Visit(node.Arguments[0]);

            var token = node.Method.UniqueToken();
            if (!queryableMethodToVisitMethodDictionary.ContainsKey(token))
                throw new NotImplementedException(String.Format("{0} is not implemented.", node.Method.Name));

            var visitMethod = queryableMethodToVisitMethodDictionary[token];
            var visitMethodInstance = visitMethod.IsGenericMethod
                                          ? visitMethod.MakeGenericMethod(node.Method.GetGenericArguments())
                                          : visitMethod;

            try
            {
                visitMethodInstance.Invoke(
                    this,
                    node.Arguments.Skip(1)
                        .Select(ExtractArgumentFromExpression)
                        .ToArray());

            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
            return node;
        }

        internal void QToUri<TSource>()
        {
            projection = QueryProjection.ToUri;
        }


        internal void QAny<TSource>()
        {
            takeCount = 1;
            projection = QueryProjection.Any;
        }


        internal void QAny<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            // TODO: When count is supported it will work better..
            QWhere(predicate);
            QAny<TSource>();
        }


        internal void QSum<TSource>(Expression<Func<TSource, int>> propertySelector)
        {
            QSelect(propertySelector);
            QSum();
        }

        internal void QSum<TSource>(Expression<Func<TSource, decimal>> propertySelector)
        {
            QSelect(propertySelector);
            QSum();
        }

        internal void QSum<TSource>(Expression<Func<TSource, double>> propertySelector)
        {
            QSelect(propertySelector);
            QSum();
        }

        internal void QToJson()
        {
            projection = QueryProjection.ToJson;
        }

        internal void QSum()
        {
            projection = QueryProjection.Sum;
        }

        internal void QExpand<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (expandedPaths.Length > 0)
                expandedPaths.Append(',');
            expandedPaths.Append(propertySelector.GetPropertyPath(true));
        }


        internal void QFirst<TSource>()
        {
            projection = QueryProjection.First;
        }

        internal void QMax<TSource>()
        {
            projection = QueryProjection.Max;
        }

        internal void QMax<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            QSelect(selector);
            QMax<TResult>();
        }

        internal void QCount<TSource>()
        {
            projection = QueryProjection.Count;
            aggregateReturnType = typeof (int);
        }

        internal void QCount<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QCount<TSource>();
        }

        internal void QMin<TSource>()
        {
            projection = QueryProjection.Min;
        }

        internal void QMin<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            QSelect(selector);
            QMin<TResult>();
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
            projection = QueryProjection.FirstOrDefault;
        }

        internal void QFirstLazy<TSource>()
        {
            projection = QueryProjection.FirstLazy;
        }


        internal void QFirstOrDefault<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QFirstOrDefault<TSource>();
        }


        internal void QThenBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            OrderBy(keySelector, SortOrder.Ascending, true);
        }


        internal void QThenByDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            OrderBy(keySelector, SortOrder.Descending, true);
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

        internal void QOfType<TResult>()
        {
            if (!elementType.IsAssignableFrom(typeof (TResult)))
                throw new NotSupportedException("Only supports OfType'ing to inherited type.");

            if (selectExpression != null)
                throw new NotSupportedException("Does only support OfType at start of query.");

            if (wherePredicate != null)
            {
                var newParam = Expression.Parameter(typeof (TResult), wherePredicate.Parameters[0].Name);
                var replacer = new LamdbaParameterReplacer(wherePredicate.Parameters[0], newParam);
                wherePredicate = Expression.Lambda(replacer.Visit(wherePredicate.Body), newParam);
            }

            elementType = typeof (TResult);
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
            if (expandedPaths.Length > 0)
                throw new NotSupportedException("Pomona LINQ provider does not support using Expand() before Select()");
            selectExpression = selectExpression != null ? MergeWhereAfterSelect(selector) : selector;
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

        private LambdaExpression MergeWhereAfterSelect(LambdaExpression predicate)
        {
            var parameter = Expression.Parameter(selectExpression.Parameters[0].Type,
                                                 selectExpression.Parameters[0].Name);
            var fixedSelectExpr = LamdbaParameterReplacer.Replace(selectExpression.Body, selectExpression.Parameters[0],
                                                                  parameter);
            var expandedBody = LamdbaParameterReplacer.Replace(predicate.Body, predicate.Parameters[0], fixedSelectExpr);
            var newBody = new CollapseDisplayObjectsVisitor().Visit(expandedBody);
            return Expression.Lambda(newBody, parameter);
        }

        internal void QWhere<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            LambdaExpression fixedPredicate = predicate;

            if (GroupByKeySelector != null)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Where() after GroupBy()");
            if (SelectExpression != null)
                fixedPredicate = MergeWhereAfterSelect(fixedPredicate);

            whereExpressions.Add(fixedPredicate);
            if (wherePredicate == null)
                wherePredicate = fixedPredicate;
            else
            {
                var replacer = new LamdbaParameterReplacer(fixedPredicate.Parameters[0], wherePredicate.Parameters[0]);
                var rewrittenPredicateBody = replacer.Visit(fixedPredicate.Body);
                wherePredicate = Expression.Lambda(
                    wherePredicate.Type,
                    Expression.AndAlso(wherePredicate.Body, rewrittenPredicateBody),
                    wherePredicate.Parameters);
            }
        }


        private static void MapQueryableFunction(Expression<Action<IQueryable<int>>> expr)
        {
            MapQueryableFunction(ReflectionHelper.GetMethodDefinition(expr));
        }


        private static bool TryMapQueryableFunction(MethodInfo method)
        {
            var visitMethod = typeof (RestQueryableTreeParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => VisitMethodMatches(x, method));

            if (visitMethod == null)
                return false;

            queryableMethodToVisitMethodDictionary.Add(method.UniqueToken(), visitMethod);
            return true;
        }

        private static void MapQueryableFunction(MethodInfo method)
        {
            if (!TryMapQueryableFunction(method))
                throw new InvalidOperationException("Unable to find visitmethod to handle " + method.Name);
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


        private void OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector, SortOrder sortOrder, bool thenBy = false)
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

            if (!thenBy)
                orderKeySelectors.Clear();

            if (selectExpression != null && groupByKeySelector == null)
            {
                // Support order by after select (not when using GroupBy)
                orderKeySelectors.Add(new Tuple<LambdaExpression, SortOrder>(MergeWhereAfterSelect(keySelector), sortOrder));
            }
            else
            {
                orderKeySelectors.Add(new Tuple<LambdaExpression, SortOrder>(keySelector, sortOrder));
            }
        }


        #region Nested type: LamdbaParameterReplacer

        private class LamdbaParameterReplacer : ExpressionVisitor
        {
            private readonly Expression replaceParam;
            private readonly ParameterExpression searchParam;


            public LamdbaParameterReplacer(ParameterExpression searchParam, Expression replaceParam)
            {
                this.searchParam = searchParam;
                this.replaceParam = replaceParam;
            }

            public static Expression Replace(Expression target, ParameterExpression searchParam, Expression replaceParam)
            {
                return (new LamdbaParameterReplacer(searchParam, replaceParam)).Visit(target);
            }


            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == searchParam)
                    return replaceParam;
                return base.VisitParameter(node);
            }
        }

        #endregion

        private class CollapseDisplayObjectsVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                var newExprNode = node.Expression as NewExpression;
                if (newExprNode != null)
                {
                    var memberIndex = newExprNode.Members.IndexOf(node.Member);
                    if (memberIndex != -1)
                    {
                        return Visit(newExprNode.Arguments[memberIndex]);
                    }
                }

                return base.VisitMember(node);
            }
        }
    }
}