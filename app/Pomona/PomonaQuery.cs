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
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Internals;

namespace Pomona
{
    /// <summary>
    /// A default implementation of PomonaQuery, only simple querying.
    /// </summary>
    public class PomonaQuery
    {
        public enum ProjectionType
        {
            Default,
            First,
            FirstOrDefault,
            Max,
            Min,
            Sum
        }

        private static readonly MethodInfo applyAndExecuteMethod;
        private readonly IPomonaUriResolver uriResolver;
        private readonly TransformedType targetType;


        static PomonaQuery()
        {
            applyAndExecuteMethod =
                ReflectionHelper.GetMethodDefinition<PomonaQuery>(x => x.ApplyAndExecute<object>(null, false));
        }


        public PomonaQuery(TransformedType targetType, IPomonaUriResolver uriResolver)
        {
            DebugInfoKeys = new HashSet<string>();
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            if (uriResolver == null) throw new ArgumentNullException("uriResolver");
            this.targetType = targetType;
            this.uriResolver = uriResolver;
        }

        public HashSet<string> DebugInfoKeys { get; set; }

        public LambdaExpression FilterExpression { get; set; }
        public LambdaExpression GroupByExpression { get; set; }

        public LambdaExpression OrderByExpression { get; set; }
        public LambdaExpression SelectExpression { get; set; }

        public ProjectionType Projection { get; set; }
        public int Skip { get; set; }
        public SortOrder SortOrder { get; set; }
        public int Top { get; set; }

        public bool IncludeTotalCount { get; set; }

        #region PomonaQuery Members

        public IMappedType ResultType { get; internal set; }
        public string ExpandedPaths { get; set; }

        public TransformedType TargetType
        {
            get { return targetType; }
        }

        public string Url { get; set; }

        #endregion

        public bool DebugEnabled(string debugKey)
        {
            return DebugInfoKeys.Contains(debugKey.ToLower());
        }

        public PomonaResponse ApplyAndExecute(IQueryable queryable, bool skipAndTakeAfterExecute = false)
        {
            var totalQueryable = ApplyExpressions(queryable);
            return (PomonaResponse) applyAndExecuteMethod.MakeGenericMethod(totalQueryable.ElementType).Invoke(
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


        private PomonaResponse ApplyAndExecute<T>(IQueryable<T> totalQueryable, bool skipAndTakeAfterExecute)
        {
            switch (Projection)
            {
                case ProjectionType.First:
                    return new PomonaResponse(this, totalQueryable.First(), uriResolver);
                case ProjectionType.FirstOrDefault:
                    return new PomonaResponse(this, totalQueryable.FirstOrDefault(), uriResolver);
                case ProjectionType.Max:
                    return new PomonaResponse(this, totalQueryable.Max(), uriResolver);
                case ProjectionType.Min:
                    return new PomonaResponse(this, totalQueryable.Min(), uriResolver);
                case ProjectionType.Sum:
                    return ApplySum(totalQueryable);
                default:
                    {
                        IList<T> limitedQueryable;
                        var totalCount = IncludeTotalCount ? totalQueryable.Count() : -1;
                        if (skipAndTakeAfterExecute)
                        {
                            limitedQueryable = ((IEnumerable<T>) (totalQueryable)).Skip(Skip).Take(Top).ToList();
                        }
                        else
                            limitedQueryable = ((IQueryable<T>) ApplySkipAndTake(totalQueryable)).ToList();

                        var qr = QueryResult.Create(limitedQueryable, Skip, totalCount, Url);
                        return new PomonaResponse(this, qr, uriResolver);
                    }
            }
        }

        private PomonaResponse ApplySum<T>(IQueryable<T> totalQueryable)
        {
            var intQueryable = totalQueryable as IQueryable<int>;
            if (intQueryable != null)
                return new PomonaResponse(this, intQueryable.Sum(), uriResolver);
            var nullableIntQueryable = totalQueryable as IQueryable<int?>;
            if (nullableIntQueryable != null)
                return new PomonaResponse(this, nullableIntQueryable.Sum(), uriResolver);
            var decimalQueryable = totalQueryable as IQueryable<decimal>;
            if (decimalQueryable != null)
                return new PomonaResponse(this, decimalQueryable.Sum(), uriResolver);
            var nullableDecimalQueryable = totalQueryable as IQueryable<decimal?>;
            if (nullableDecimalQueryable != null)
                return new PomonaResponse(this, nullableDecimalQueryable.Sum(), uriResolver);
            var doubleQueryable = totalQueryable as IQueryable<double>;
            if (doubleQueryable != null)
                return new PomonaResponse(this, doubleQueryable.Sum(), uriResolver);
            var nullableDoubleQueryable = totalQueryable as IQueryable<double?>;
            if (nullableDoubleQueryable != null)
                return new PomonaResponse(this, nullableDoubleQueryable.Sum(), uriResolver);

            throw new NotSupportedException("Unable to calculate sum of type " + typeof (T).Name);
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