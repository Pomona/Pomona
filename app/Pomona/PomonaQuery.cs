#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

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
        public QueryProjection Projection { get; set; }
        public TypeSpec ResultType { get; internal set; }
        public LambdaExpression SelectExpression { get; set; }
        public int Skip { get; set; }

        public StructuredType SourceType { get; }

        public int Top { get; set; }
        public string Url { get; set; }

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

        public PomonaResponse ApplyAndExecute(IQueryable queryable, bool skipAndTakeAfterExecute = false)
        {
            queryable = this.ApplyExpressions(queryable);
            var projection = this.Projection;
            if (projection == QueryProjection.AsEnumerable)
            {
                IList limitedQueryable;
                var totalCount = this.IncludeTotalCount ? (int)queryable.Execute(QueryProjection.Count) : -1;

                if (skipAndTakeAfterExecute)
                {
                    limitedQueryable = ((IEnumerable)queryable)
                        .Cast<object>()
                        .Skip(Skip)
                        .Take(Top)
                        .Cast(queryable.ElementType)
                        .ToListDetectType();
                }
                else
                {
                    if (this.Skip > 0)
                        queryable = queryable.Skip(this.Skip);
                    queryable = queryable.Take(this.Top);
                    limitedQueryable = queryable.ToListDetectType();
                }

                var previous = GetPageLink(this.Url, this.Skip, this.Top, limitedQueryable.Count, totalCount, -1);
                var next = GetPageLink(this.Url, this.Skip, this.Top, limitedQueryable.Count, totalCount, 1);

                var qr = QueryResult.Create(limitedQueryable, this.Skip, totalCount, previous, next);
                return new PomonaResponse(this, qr);
            }

            try
            {
                var result = queryable.Execute(projection);
                return new PomonaResponse(this, result);
            }
            catch (InvalidOperationException)
                when (projection == QueryProjection.First || projection == QueryProjection.Last || projection == QueryProjection.Single)
            {
                // We assume that this means no matching element.
                // Don't know another way to check this in a non-ambigious way, since null might be a valid return value.
                return new PomonaResponse(this,
                                          PomonaResponse.NoBodyEntity,
                                          HttpStatusCode.NotFound);
            }
        }


        private static string GetPageLink(string url, int skip, int take, int count, int totalcount, int offset)
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


        public bool DebugEnabled(string debugKey)
        {
            return DebugInfoKeys.Contains(debugKey.ToLower());
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
    }
}