#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
            Single,
            SingleOrDefault,
            Any,
            Max,
            Min,
            Sum,
            Count
        }

        #endregion

        public enum ResultModeType
        {
            Deserialized,
            ToJson,
            ToUri
        }

        private static readonly Dictionary<UniqueMemberToken, MethodInfo> queryableMethodToVisitMethodDictionary;
        private readonly StringBuilder expandedPaths;
        private readonly List<Tuple<LambdaExpression, SortOrder>> orderKeySelectors;
        private readonly List<Action<IRequestOptions>> requestOptionActions;
        private readonly IList<LambdaExpression> whereExpressions;
        private Type aggregateReturnType;
        private Type elementType;
        private LambdaExpression groupByKeySelector;
        private bool includeTotalCount;
        private QueryProjection projection;
        private ResultModeType resultMode = ResultModeType.Deserialized;
        private IRestQueryRoot queryRoot;
        private LambdaExpression selectExpression;
        private int? skipCount;
        private int? takeCount;
        private LambdaExpression wherePredicate;


        static RestQueryableTreeParser()
        {
            queryableMethodToVisitMethodDictionary = new Dictionary<UniqueMemberToken, MethodInfo>();

            foreach (var method in typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static))
                TryMapQueryableFunction(method);

            MapQueryableFunction(x => x.Expand(y => 0));
            MapQueryableFunction(x => x.ExpandShallow(y => (object[])null));
            MapQueryableFunction(x => x.IncludeTotalCount());
            MapQueryableFunction(x => x.ToUri());
            MapQueryableFunction(x => x.FirstLazy());
            MapQueryableFunction(x => x.ToJson());
            MapQueryableFunction(x => x.WithOptions(null));
            MapQueryableFunction(x => x.ToQueryResult());
        }


        public RestQueryableTreeParser()
        {
            this.expandedPaths = new StringBuilder();
            this.orderKeySelectors = new List<Tuple<LambdaExpression, SortOrder>>();
            this.requestOptionActions = new List<Action<IRequestOptions>>();
            this.whereExpressions = new List<LambdaExpression>();
            this.projection = QueryProjection.Enumerable;
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

        public bool IncludeTotalCount
        {
            get { return this.includeTotalCount; }
        }

        public List<Tuple<LambdaExpression, SortOrder>> OrderKeySelectors
        {
            get { return this.orderKeySelectors; }
        }

        public QueryProjection Projection
        {
            get { return this.projection; }
        }

        public string RepositoryUri
        {
            get { return this.queryRoot.Uri; }
        }

        public List<Action<IRequestOptions>> RequestOptionActions
        {
            get { return this.requestOptionActions; }
        }

        public ResultModeType ResultMode
        {
            get { return this.resultMode; }
        }

        public LambdaExpression SelectExpression
        {
            get { return this.selectExpression; }
        }

        public Type SelectReturnType
        {
            get
            {
                if (this.aggregateReturnType != null)
                    return this.aggregateReturnType;

                if (this.selectExpression == null)
                    return ElementType;
                return this.selectExpression.ReturnType;
            }
        }

        public int? SkipCount
        {
            get { return this.skipCount; }
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
            {
                var methodName = node.Method.GetFullNameWithSignature();
                throw new NotImplementedException(String.Format("{0} is not implemented.", methodName));
            }

            var visitMethod = queryableMethodToVisitMethodDictionary[token];
            var visitMethodInstance = visitMethod.IsGenericMethod
                ? visitMethod.MakeGenericMethod(node.Method.GetGenericArguments())
                : visitMethod;

            try
            {
                var parameters = node.Arguments
                    .Skip(1)
                    .Select(ExtractArgumentFromExpression)
                    .ToArray();

                visitMethodInstance.Invoke(this, parameters);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
            return node;
        }


        internal void QAny<TSource>()
        {
            this.takeCount = 1;
            this.projection = QueryProjection.Any;
        }


        internal void QAny<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            // TODO: When count is supported it will work better..
            QWhere(predicate);
            QAny<TSource>();
        }


        internal void QCount<TSource>()
        {
            this.projection = QueryProjection.Count;
            this.aggregateReturnType = typeof(int);
        }


        internal void QCount<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QCount<TSource>();
        }


        internal void QExpand<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (this.expandedPaths.Length > 0)
                this.expandedPaths.Append(',');
            this.expandedPaths.Append(propertySelector.GetPropertyPath(true));
        }


        internal void QExpandShallow<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (this.expandedPaths.Length > 0)
                this.expandedPaths.Append(',');
            this.expandedPaths.Append(propertySelector.GetPropertyPath(true) + "!");
        }


        internal void QFirst<TSource>()
        {
            this.projection = QueryProjection.First;
        }


        internal void QFirst<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QFirst<TSource>();
        }


        internal void QFirstLazy<TSource>()
        {
            this.projection = QueryProjection.FirstLazy;
        }


        internal void QFirstOrDefault<TSource>()
        {
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

            if (this.orderKeySelectors.Any())
                throw new NotSupportedException("Pomona LINQ provider does not support calling OrderBy before GroupBy()");

            this.groupByKeySelector = keySelector;
        }


        internal void QIncludeTotalCount<TSource>()
        {
            this.includeTotalCount = true;
        }


        internal void QMax<TSource>()
        {
            this.projection = QueryProjection.Max;
        }


        internal void QMax<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            QSelect(selector);
            QMax<TResult>();
        }


        internal void QMin<TSource>()
        {
            this.projection = QueryProjection.Min;
        }


        internal void QMin<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            QSelect(selector);
            QMin<TResult>();
        }


        internal void QOfType<TResult>()
        {
            if (!this.elementType.IsAssignableFrom(typeof(TResult)))
                throw new NotSupportedException("Only supports OfType'ing to inherited type.");

            if (this.selectExpression != null)
                throw new NotSupportedException("Does only support OfType at start of query.");

            if (this.wherePredicate != null)
            {
                var newParam = Expression.Parameter(typeof(TResult), this.wherePredicate.Parameters[0].Name);
                var replacer = new LamdbaParameterReplacer(this.wherePredicate.Parameters[0], newParam);
                this.wherePredicate = Expression.Lambda(replacer.Visit(this.wherePredicate.Body), newParam);
            }

            this.elementType = typeof(TResult);
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
            QSelect((LambdaExpression)selector);
        }


        internal void QSelect(LambdaExpression selector)
        {
            if (this.expandedPaths.Length > 0)
                throw new NotSupportedException("Pomona LINQ provider does not support using Expand() before Select()");
            this.selectExpression = this.selectExpression != null ? MergeWhereAfterSelect(selector) : selector;
        }


        internal void QSingle<TSource>()
        {
            this.projection = QueryProjection.Single;
        }


        internal void QSingle<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QSingle<TSource>();
        }


        internal void QSingleOrDefault<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            QWhere(predicate);
            QSingleOrDefault<TSource>();
        }


        internal void QSingleOrDefault<TSource>()
        {
            this.projection = QueryProjection.SingleOrDefault;
        }


        internal void QSkip<TSource>(int skipCount)
        {
            if (TakeCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Take() before Skip().");
            if (SkipCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support multiple calls to Skip()");
            this.skipCount = skipCount;
        }


        internal void QSum<TSource>(Expression<Func<TSource, int>> propertySelector)
        {
            Sum(propertySelector);
        }


        internal void QSum<TSource>(Expression<Func<TSource, decimal>> propertySelector)
        {
            Sum(propertySelector);
        }


        internal void QSum<TSource>(Expression<Func<TSource, double>> propertySelector)
        {
            Sum(propertySelector);
        }


        internal void QSum<TSource>(Expression<Func<TSource, int?>> propertySelector)
        {
            Sum(propertySelector);
        }


        internal void QSum<TSource>(Expression<Func<TSource, decimal?>> propertySelector)
        {
            Sum(propertySelector);
        }


        internal void QSum<TSource>(Expression<Func<TSource, double?>> propertySelector)
        {
            Sum(propertySelector);
        }


        internal void QSum()
        {
            Sum();
        }


        internal void QTake<TSource>(int takeCount)
        {
            if (TakeCount.HasValue)
                throw new NotSupportedException("Pomona LINQ provider does not support multiple calls to Take()");
            this.takeCount = takeCount;
        }


        internal void QThenBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            OrderBy(keySelector, SortOrder.Ascending, true);
        }


        internal void QThenByDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            OrderBy(keySelector, SortOrder.Descending, true);
        }


        internal void QToJson()
        {
            this.projection = QueryProjection.Enumerable;
            this.resultMode = ResultModeType.ToJson;
        }


        internal void QToQueryResult<TSource>()
        {
            this.projection = QueryProjection.Enumerable;
        }


        internal void QToUri<TSource>()
        {
            this.projection = QueryProjection.Enumerable;
            this.resultMode = ResultModeType.ToUri;
        }


        internal void QWhere<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            LambdaExpression fixedPredicate = predicate;

            if (GroupByKeySelector != null)
                throw new NotSupportedException("Pomona LINQ provider does not support calling Where() after GroupBy()");
            if (SelectExpression != null)
                fixedPredicate = MergeWhereAfterSelect(fixedPredicate);

            this.whereExpressions.Add(fixedPredicate);
            if (this.wherePredicate == null)
                this.wherePredicate = fixedPredicate;
            else
            {
                var replacer = new LamdbaParameterReplacer(fixedPredicate.Parameters[0],
                                                           this.wherePredicate.Parameters[0]);
                var rewrittenPredicateBody = replacer.Visit(fixedPredicate.Body);
                this.wherePredicate = Expression.Lambda(
                    this.wherePredicate.Type,
                    Expression.AndAlso(this.wherePredicate.Body, rewrittenPredicateBody),
                    this.wherePredicate.Parameters);
            }
        }


        internal void QWithOptions<TSource>(Action<IRequestOptions> options)
        {
            this.requestOptionActions.Add(options);
        }


        private static void MapQueryableFunction(Expression<Action<IQueryable<int>>> expr)
        {
            MapQueryableFunction(ReflectionHelper.GetMethodDefinition(expr));
        }


        private static void MapQueryableFunction(MethodInfo method)
        {
            if (!TryMapQueryableFunction(method))
                throw new InvalidOperationException("Unable to find visitmethod to handle " + method.Name);
        }


        private static bool TryMapQueryableFunction(MethodInfo method)
        {
            var visitMethod = typeof(RestQueryableTreeParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => VisitMethodMatches(x, method));

            if (visitMethod == null)
                return false;

            queryableMethodToVisitMethodDictionary.Add(method.UniqueToken(), visitMethod);
            return true;
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


        private LambdaExpression MergeWhereAfterSelect(LambdaExpression predicate)
        {
            var parameter = Expression.Parameter(this.selectExpression.Parameters[0].Type,
                                                 this.selectExpression.Parameters[0].Name);
            var fixedSelectExpr = LamdbaParameterReplacer.Replace(this.selectExpression.Body,
                                                                  this.selectExpression.Parameters[0],
                                                                  parameter);
            var expandedBody = LamdbaParameterReplacer.Replace(predicate.Body, predicate.Parameters[0], fixedSelectExpr);
            var newBody = new CollapseDisplayObjectsVisitor().Visit(expandedBody);
            return Expression.Lambda(newBody, parameter);
        }


        private void OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector,
                                            SortOrder sortOrder,
                                            bool thenBy = false)
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
                this.orderKeySelectors.Clear();

            if (this.selectExpression != null && this.groupByKeySelector == null)
            {
                // Support order by after select (not when using GroupBy)
                this.orderKeySelectors.Add(new Tuple<LambdaExpression, SortOrder>(MergeWhereAfterSelect(keySelector),
                                                                                  sortOrder));
            }
            else
                this.orderKeySelectors.Add(new Tuple<LambdaExpression, SortOrder>(keySelector, sortOrder));
        }


        private void Sum(LambdaExpression propertySelector = null)
        {
            if (propertySelector != null)
                QSelect(propertySelector);
            this.projection = QueryProjection.Sum;
        }

        #region Nested type: CollapseDisplayObjectsVisitor

        private class CollapseDisplayObjectsVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                var newExprNode = node.Expression as NewExpression;
                if (newExprNode != null)
                {
                    var memberIndex = newExprNode.Members.IndexOf(node.Member);
                    if (memberIndex != -1)
                        return Visit(newExprNode.Arguments[memberIndex]);
                }

                return base.VisitMember(node);
            }
        }

        #endregion

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
                return new LamdbaParameterReplacer(searchParam, replaceParam)
                    .Visit(target);
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