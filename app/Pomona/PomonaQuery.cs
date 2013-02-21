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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Internals;

namespace Pomona
{
    /// <summary>
    /// A default implementation of IPomonaQuery, only simple querying.
    /// </summary>
    public class PomonaQuery : IPomonaQuery
    {
        private static readonly MethodInfo applyAndExecuteMethod;
        private readonly TransformedType targetType;


        static PomonaQuery()
        {
            applyAndExecuteMethod =
                ReflectionHelper.GetGenericMethodDefinition<PomonaQuery>(x => x.ApplyAndExecute<object>(null, false));
        }


        public PomonaQuery(TransformedType targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            this.targetType = targetType;
        }


        public LambdaExpression FilterExpression { get; set; }
        public LambdaExpression GroupByExpression { get; set; }

        public LambdaExpression OrderByExpression { get; set; }
        public LambdaExpression SelectExpression { get; set; }

        public int Skip { get; set; }
        public SortOrder SortOrder { get; set; }
        public int Top { get; set; }

        public bool IncludeTotalCount { get; set; }

        #region IPomonaQuery Members

        public string ExpandedPaths { get; set; }

        public TransformedType TargetType
        {
            get { return targetType; }
        }

        public string Url { get; set; }

        #endregion

        public QueryResult ApplyAndExecute(IQueryable queryable, bool skipAndTakeAfterExecute = false)
        {
            var totalQueryable = ApplyExpressions(queryable);
            return (QueryResult) applyAndExecuteMethod.MakeGenericMethod(totalQueryable.ElementType).Invoke(
                this, new object[] {totalQueryable, skipAndTakeAfterExecute});
        }


        public IQueryable ApplyExpressions(IQueryable queryable)
        {
            queryable =
                (IQueryable)
                QueryableMethods.Where.MakeGenericMethod(queryable.ElementType).Invoke(
                    null, new object[] {queryable, FilterExpression});

            if (GroupByExpression == null)
            {
                // OrderBy is applied BEFORE select if GroupBy has not been specified.
                queryable = ApplyOrderByExpression(queryable);
            }
            else
            {
                queryable = (IQueryable) QueryableMethods.GroupBy
                                                         .MakeGenericMethod(queryable.ElementType,
                                                                            GroupByExpression.ReturnType)
                                                         .Invoke(null, new object[] {queryable, GroupByExpression});
            }

            if (SelectExpression != null)
            {
                queryable = (IQueryable) QueryableMethods.Select
                                                         .MakeGenericMethod(queryable.ElementType,
                                                                            SelectExpression.ReturnType)
                                                         .Invoke(null, new object[] {queryable, SelectExpression});

                // OrderBy is applied AFTER select if GroupBy has been specified.
                if (GroupByExpression != null)
                    queryable = ApplyOrderByExpression(queryable);
            }
            else if (GroupByExpression != null)
            {
                throw new PomonaExpressionSyntaxException(
                    "Query error: $groupby has to be combined with a $select query parameter.");
            }

            return queryable;
        }


        public IQueryable ApplySkipAndTake(IQueryable queryable)
        {
            if (Skip > 0)
            {
                queryable = (IQueryable)
                            QueryableMethods.Skip.MakeGenericMethod(queryable.ElementType).Invoke(
                                null, new object[] {queryable, Skip});
            }

            queryable = (IQueryable)
                        QueryableMethods.Take.MakeGenericMethod(queryable.ElementType)
                                        .Invoke(null, new object[] {queryable, Top});
            return queryable;
        }


        private QueryResult ApplyAndExecute<T>(IQueryable<T> totalQueryable, bool skipAndTakeAfterExecute)
        {
            IEnumerable limitedQueryable;
            var totalCount = IncludeTotalCount ? totalQueryable.Count() : -1;
            if (skipAndTakeAfterExecute)
            {
                limitedQueryable = ((IEnumerable<T>) (totalQueryable)).Skip(Skip).Take(Top);
            }
            else
                limitedQueryable = ((IQueryable<T>) ApplySkipAndTake(totalQueryable)).ToList();

            return QueryResult.Create(limitedQueryable, Skip, totalCount, Url);
        }


        private IQueryable ApplyOrderByExpression(IQueryable queryable)
        {
            if (OrderByExpression != null)
            {
                var orderMethod = SortOrder == SortOrder.Descending
                                      ? QueryableMethods.OrderByDescending
                                      : QueryableMethods.OrderBy;

                queryable = (IQueryable) orderMethod
                                             .MakeGenericMethod(queryable.ElementType, OrderByExpression.ReturnType)
                                             .Invoke(null, new object[] {queryable, OrderByExpression});
            }
            return queryable;
        }
    }
}