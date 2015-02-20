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

using Nancy;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Linq.NonGeneric;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    /// <summary>
    /// A default implementation of PomonaQuery, only simple querying.
    /// </summary>
    public class PomonaQuery
    {
        #region ProjectionType enum

        public enum ProjectionType
        {
            Default,
            First,
            FirstOrDefault,
            Single,
            SingleOrDefault,
            Max,
            Min,
            Sum,
            Count
        }

        #endregion

        private static readonly Func<Type, PomonaQuery, IQueryable, bool, PomonaResponse> applyAndExecuteMethod;
        private readonly StructuredType ofType;
        private readonly StructuredType sourceType;


        static PomonaQuery()
        {
            applyAndExecuteMethod = GenericInvoker
                .Instance<PomonaQuery>()
                .CreateFunc1<IQueryable, bool, PomonaResponse>(x => x.ApplyAndExecute<object>(null, false));
        }


        public PomonaQuery(StructuredType sourceType, StructuredType ofType = null)
        {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");

            OrderByExpressions = new List<Tuple<LambdaExpression, SortOrder>>();
            this.sourceType = sourceType;
            this.ofType = ofType ?? sourceType;
            DebugInfoKeys = new HashSet<string>();
        }


        public HashSet<string> DebugInfoKeys { get; set; }
        public string ExpandedPaths { get; set; }
        public LambdaExpression FilterExpression { get; set; }
        public LambdaExpression GroupByExpression { get; set; }
        public bool IncludeTotalCount { get; set; }

        public StructuredType OfType
        {
            get { return this.ofType; }
        }

        public List<Tuple<LambdaExpression, SortOrder>> OrderByExpressions { get; set; }
        public ProjectionType Projection { get; set; }
        public TypeSpec ResultType { get; internal set; }
        public LambdaExpression SelectExpression { get; set; }
        public int Skip { get; set; }

        public StructuredType SourceType
        {
            get { return this.sourceType; }
        }

        public int Top { get; set; }
        public string Url { get; set; }


        public PomonaResponse ApplyAndExecute(IQueryable queryable, bool skipAndTakeAfterExecute = false)
        {
            var totalQueryable = ApplyExpressions(queryable);
            return applyAndExecuteMethod(totalQueryable.ElementType, this, totalQueryable, skipAndTakeAfterExecute);
        }


        public IQueryable ApplyExpressions(IQueryable queryable)
        {
            if (queryable.ElementType != OfType.Type)
                queryable = queryable.OfType(this.ofType.Type);

            queryable = queryable.Where(FilterExpression);

            if (GroupByExpression == null)
            {
                // OrderBy is applied BEFORE select if GroupBy has not been specified.
                queryable = ApplyOrderByExpression(queryable);
            }
            else
                queryable = queryable.GroupBy(GroupByExpression);

            if (SelectExpression != null)
            {
                queryable = queryable.Select(SelectExpression);

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
                queryable = queryable.Skip(Skip);

            queryable = queryable.Take(Top);
            return queryable;
        }


        public bool DebugEnabled(string debugKey)
        {
            return DebugInfoKeys.Contains(debugKey.ToLower());
        }


        private PomonaResponse ApplyAndExecute<T>(IQueryable<T> totalQueryable, bool skipAndTakeAfterExecute)
        {
            switch (Projection)
            {
                case ProjectionType.Single:
                case ProjectionType.First:
                {
                    object result;
                    try
                    {
                        result = Projection == ProjectionType.First ? totalQueryable.First() : totalQueryable.Single();
                    }
                    catch (InvalidOperationException)
                    {
                        // We assume that this means no matching element.
                        // Don't know another way to check this in a non-ambigious way, since null might be a valid return value.
                        return new PomonaResponse(this,
                                                  PomonaResponse.NoBodyEntity,
                                                  HttpStatusCode.NotFound);
                    }
                    return new PomonaResponse(this, result);
                }
                case ProjectionType.FirstOrDefault:
                    return new PomonaResponse(this, totalQueryable.FirstOrDefault());
                case ProjectionType.SingleOrDefault:
                    return new PomonaResponse(this, totalQueryable.SingleOrDefault());
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
                        limitedQueryable = ((IEnumerable<T>)(totalQueryable)).Skip(Skip).Take(Top).ToList();
                    else
                        limitedQueryable = ((IQueryable<T>)ApplySkipAndTake(totalQueryable)).ToList();

                    var qr = QueryResult.Create(limitedQueryable, Skip, totalCount, Url);
                    return new PomonaResponse(this, qr);
                }
            }
        }


        private IQueryable ApplyOrderByExpression(IQueryable queryable)
        {
            var first = true;
            foreach (var tuple in OrderByExpressions)
            {
                if (first)
                {
                    queryable = queryable.OrderBy(tuple.Item1, tuple.Item2);
                    first = false;
                }
                else
                    queryable = ((IOrderedQueryable)queryable).ThenBy(tuple.Item1, tuple.Item2);
            }
            return queryable;
        }


        private PomonaResponse ApplySum<T>(IQueryable<T> totalQueryable)
        {
            return new PomonaResponse(this, totalQueryable.Sum());
        }
    }
}