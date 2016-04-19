#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
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
            Count,
            Last,
            LastOrDefault,
            Any
        }

        #endregion

        private static readonly Func<Type, PomonaQuery, IQueryable, bool, PomonaResponse> applyAndExecuteMethod;


        static PomonaQuery()
        {
            applyAndExecuteMethod = GenericInvoker
                .Instance<PomonaQuery>()
                .CreateFunc1<IQueryable, bool, PomonaResponse>(x => x.ApplyAndExecute<object>(null, false));
        }


        public PomonaQuery(StructuredType sourceType, StructuredType ofType = null)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            OrderByExpressions = new List<Tuple<LambdaExpression, SortOrder>>();
            SourceType = sourceType;
            OfType = ofType ?? sourceType;
            DebugInfoKeys = new HashSet<string>();
        }


        public HashSet<string> DebugInfoKeys { get; set; }
        public string ExpandedPaths { get; set; }
        public LambdaExpression FilterExpression { get; set; }
        public LambdaExpression GroupByExpression { get; set; }
        public bool IncludeTotalCount { get; set; }

        public StructuredType OfType { get; }

        public List<Tuple<LambdaExpression, SortOrder>> OrderByExpressions { get; set; }
        public ProjectionType Projection { get; set; }
        public TypeSpec ResultType { get; internal set; }
        public LambdaExpression SelectExpression { get; set; }
        public int Skip { get; set; }

        public StructuredType SourceType { get; }

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
                queryable = queryable.OfType(OfType.Type);

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
                case ProjectionType.Last:
                {
                    object result = null;
                    try
                    {
                        switch (Projection)
                        {
                            case ProjectionType.First:
                                result = totalQueryable.First();
                                break;
                            case ProjectionType.Last:
                                result = totalQueryable.Last();
                                break;
                            case ProjectionType.Single:
                                result = totalQueryable.Single();
                                break;
                        }
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
                case ProjectionType.LastOrDefault:
                    return new PomonaResponse(this, totalQueryable.LastOrDefault());
                case ProjectionType.Max:
                    return new PomonaResponse(this, totalQueryable.Max());
                case ProjectionType.Min:
                    return new PomonaResponse(this, totalQueryable.Min());
                case ProjectionType.Any:
                    return new PomonaResponse(this, totalQueryable.Any());
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

                    var previous = GetPage(Url, Skip, Top, limitedQueryable.Count, totalCount, -1);
                    var next = GetPage(Url, Skip, Top, limitedQueryable.Count, totalCount, 1);

                    var qr = QueryResult.Create(limitedQueryable, Skip, totalCount, previous, next);
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


        private static string GetPage(string url, int skip, int take, int count, int totalcount, int offset)
        {
            var newSkip = Math.Max(skip + (take * offset), 0);
            var uriBuilder = new UriBuilder(url);

            if (skip == newSkip || (totalcount != -1 && newSkip >= totalcount) || count < take)
                return null;

            NameValueCollection parameters;
            if (!string.IsNullOrEmpty(uriBuilder.Query))
            {
                parameters = HttpUtility.ParseQueryString(uriBuilder.Query);
                parameters["$skip"] = newSkip.ToString(CultureInfo.InvariantCulture);
                uriBuilder.Query = parameters.ToString();
            }
            else
                uriBuilder.Query = "$skip=" + newSkip;

            return uriBuilder.Uri.ToString();
        }
    }
}