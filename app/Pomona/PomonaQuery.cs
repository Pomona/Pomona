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
using Nancy;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

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
            Sum,
            Count
        }

        private static readonly Func<Type, PomonaQuery, IQueryable, bool, PomonaResponse> applyAndExecuteMethod;
        private readonly TransformedType sourceType;

        public TransformedType SourceType
        {
            get { return this.sourceType; }
        }

        private readonly TransformedType ofType;


        static PomonaQuery()
        {
            applyAndExecuteMethod =
                GenericInvoker.Instance<PomonaQuery>().CreateFunc1<IQueryable, bool, PomonaResponse>(x => x.ApplyAndExecute<object>(null, false));
        }

        
        public PomonaQuery(TransformedType sourceType, TransformedType ofType = null)
        {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");
            this.sourceType = sourceType;
            this.ofType = ofType ?? sourceType;
            DebugInfoKeys = new HashSet<string>();
        }

        public HashSet<string> DebugInfoKeys { get; set; }

        public LambdaExpression FilterExpression { get; set; }
        public LambdaExpression GroupByExpression { get; set; }

        private List<Tuple<LambdaExpression, SortOrder>> orderByExpressions = new List<Tuple<LambdaExpression, SortOrder>>();

        public List<Tuple<LambdaExpression, SortOrder>> OrderByExpressions
        {
            get { return orderByExpressions; }
            set { orderByExpressions = value; }
        }

        public LambdaExpression SelectExpression { get; set; }

        public ProjectionType Projection { get; set; }
        public int Skip { get; set; }
        public int Top { get; set; }

        public bool IncludeTotalCount { get; set; }

        #region PomonaQuery Members

        public TypeSpec ResultType { get; internal set; }
        public string ExpandedPaths { get; set; }

        public TransformedType OfType
        {
            get { return this.ofType; }
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
            return applyAndExecuteMethod(totalQueryable.ElementType, this, totalQueryable, skipAndTakeAfterExecute);
        }


        public IQueryable ApplyExpressions(IQueryable queryable)
        {
            if (queryable.ElementType != OfType.Type)
            {
                queryable =
                    (IQueryable)
                        QueryableMethods.OfType.MakeGenericMethod(ofType.Type).Invoke(null,
                            new object[] { queryable });
            }

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
                    {
                        object result;
                        try
                        {
                            result = totalQueryable.First();
                        }
                        catch (InvalidOperationException)
                        {
                            // We assume that this means no matching element.
                            // Don't know another way to check this in a non-ambigious way, since null might be a valid return value.
                            return new PomonaResponse(this, PomonaResponse.NoBodyEntity,
                                                      HttpStatusCode.NotFound);
                        }
                        return new PomonaResponse(this, result);
                    }
                case ProjectionType.FirstOrDefault:
                    return new PomonaResponse(this, totalQueryable.FirstOrDefault());
                case ProjectionType.Max:
                    return new PomonaResponse(this, totalQueryable.Max());
                case ProjectionType.Min:
                    return new PomonaResponse(this, totalQueryable.Min());
                case ProjectionType.Count:
                    return new PomonaResponse(this, totalQueryable.Count());
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
                        return new PomonaResponse(this, qr);
                    }
            }
        }

        private PomonaResponse ApplySum<T>(IQueryable<T> totalQueryable)
        {
            var intQueryable = totalQueryable as IQueryable<int>;
            if (intQueryable != null)
                return new PomonaResponse(this, intQueryable.Sum());
            var nullableIntQueryable = totalQueryable as IQueryable<int?>;
            if (nullableIntQueryable != null)
                return new PomonaResponse(this, nullableIntQueryable.Sum());
            var decimalQueryable = totalQueryable as IQueryable<decimal>;
            if (decimalQueryable != null)
                return new PomonaResponse(this, decimalQueryable.Sum());
            var nullableDecimalQueryable = totalQueryable as IQueryable<decimal?>;
            if (nullableDecimalQueryable != null)
                return new PomonaResponse(this, nullableDecimalQueryable.Sum());
            var doubleQueryable = totalQueryable as IQueryable<double>;
            if (doubleQueryable != null)
                return new PomonaResponse(this, doubleQueryable.Sum());
            var nullableDoubleQueryable = totalQueryable as IQueryable<double?>;
            if (nullableDoubleQueryable != null)
                return new PomonaResponse(this, nullableDoubleQueryable.Sum());

            throw new NotSupportedException("Unable to calculate sum of type " + typeof (T).Name);
        }


        private IQueryable ApplyOrderByExpression(IQueryable queryable)
        {
            bool first = true;
            foreach (var tuple in OrderByExpressions)
            {
                MethodInfo orderMethod = null;
                if (first)
                {
                    orderMethod = tuple.Item2 == SortOrder.Descending
                        ? QueryableMethods.OrderByDescending
                        : QueryableMethods.OrderBy;
                    first = false;
                }
                else
                {
                    orderMethod = tuple.Item2 == SortOrder.Descending
                        ? QueryableMethods.ThenByDescending
                        : QueryableMethods.ThenBy;
                }
                queryable = (IQueryable)orderMethod
                                             .MakeGenericMethod(queryable.ElementType, tuple.Item1.ReturnType)
                                             .Invoke(null, new object[] { queryable, tuple.Item1 });
            }
            return queryable;
#if false
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
#endif
            throw new NotImplementedException();
        }
    }
}