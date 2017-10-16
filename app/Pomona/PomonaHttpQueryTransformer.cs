#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Linq.NonGeneric;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaHttpQueryTransformer
    {
        private readonly QueryExpressionParser parser;
        private readonly ITypeResolver typeMapper;


        public PomonaHttpQueryTransformer(ITypeResolver typeMapper, QueryExpressionParser parser)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            this.typeMapper = typeMapper;
            this.parser = parser;
        }

        #region IHttpQueryTransformer Members

        private static readonly Dictionary<string, QueryProjection> projectionMap =
            new Dictionary<string, QueryProjection>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(QueryProjection.First), QueryProjection.First },
                { nameof(QueryProjection.FirstOrDefault), QueryProjection.FirstOrDefault },
                { nameof(QueryProjection.Single), QueryProjection.Single },
                { nameof(QueryProjection.SingleOrDefault), QueryProjection.SingleOrDefault },
                { nameof(QueryProjection.Max), QueryProjection.Max },
                { nameof(QueryProjection.Min), QueryProjection.Min },
                { nameof(QueryProjection.Sum), QueryProjection.Sum },
                { nameof(QueryProjection.Count), QueryProjection.Count },
                { nameof(QueryProjection.Last), QueryProjection.Last },
                { nameof(QueryProjection.LastOrDefault), QueryProjection.LastOrDefault },
                { nameof(QueryProjection.Any), QueryProjection.Any }
            };


        public PomonaQuery TransformRequest(PomonaContext context, StructuredType rootType, int? defaultTop = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (rootType == null)
                throw new ArgumentNullException(nameof(rootType));

            StructuredType ofType = null;
            string ofTypeString;
            var qs = context.Query;
            if (qs.TryGetValue("$oftype", out ofTypeString))
                ofType = (StructuredType)this.typeMapper.FromType(ofTypeString);

            var query = new PomonaQuery(rootType, ofType);

            string debugString;
            if (qs.TryGetValue("$debug", out debugString))
            {
                query.DebugInfoKeys =
                    new HashSet<string>(debugString.ToLower().Split(',').Select(x => x.Trim()));
            }

            var culture = CultureInfo.InvariantCulture;

            query.IncludeTotalCount = qs.SafeGet("$totalcount") == "true";

            var top = qs.SafeGet("$top")?.Parse<int>(culture) ?? defaultTop ?? 100;
            var skip = qs.SafeGet("$skip")?.Parse<int>(culture) ?? 0;
            var filter = qs.SafeGet("$filter");

            ParseFilterExpression(query, filter);
            var selectSourceType = query.OfType.Type;

            string groupby;
            if (qs.TryGetValue("$groupby", out groupby))
            {
                ParseGroupByExpression(query, groupby);
                selectSourceType =
                    typeof(IGrouping<,>).MakeGenericType(
                        query.GroupByExpression.ReturnType, selectSourceType);
            }

            string projectionString;
            if (qs.TryGetValue("$projection", out projectionString))
            {
                query.Projection = projectionMap[projectionString];

                QueryProjection projection;
                if (!projectionMap.TryGetValue(projectionString, out projection))
                {
                    throw new QueryParseException("\"" + projectionString +
                                                  "\" is not a valid value for query parameter $projection",
                                                  null,
                                                  QueryParseErrorReason.UnrecognizedProjection,
                                                  null);
                }
                query.Projection = projection;
            }
            else
            {
                query.Projection = QueryProjection.AsEnumerable;
            }

            string @select;
            if (qs.TryGetValue("$select", out select))
            {
                ParseSelect(query, select, selectSourceType);
            }

            string @orderby;
            if (qs.TryGetValue("$orderby", out orderby))
                ParseOrderBy(query, orderby);

            query.Top = top;
            query.Skip = skip;

            query.ExpandedPaths = qs.SafeGet("$expand") ?? string.Empty;

            query.Url = context.Url;

            UpdateResultType(query);

            return query;
        }


        private void UpdateResultType(PomonaQuery query)
        {
            TypeSpec elementType = query.OfType;
            if (query.SelectExpression != null)
                elementType = this.typeMapper.FromType(query.SelectExpression.ReturnType);

            if (query.Projection == QueryProjection.First
                || query.Projection == QueryProjection.FirstOrDefault
                || query.Projection == QueryProjection.Single
                || query.Projection == QueryProjection.SingleOrDefault
                || query.Projection == QueryProjection.Last
                || query.Projection == QueryProjection.LastOrDefault)
                query.ResultType = elementType;
        }


        private void ParseSelect(PomonaQuery query, string select, Type thisType)
        {
            query.SelectExpression = this.parser.ParseSelectList(thisType, select);
        }

        #endregion

        private void ParseFilterExpression(PomonaQuery query, string filter)
        {
            filter = filter ?? "true";
            query.FilterExpression = this.parser.Parse(query.OfType.Type, filter);
        }


        private void ParseGroupByExpression(PomonaQuery query, string groupby)
        {
            query.GroupByExpression = this.parser.ParseSelectList(query.OfType.Type, groupby);
        }


#if false
        private Tuple<> ParseOrderByPart(string orderByPart)
        {
            const string ascMark = " asc";
            var descMark = " desc";
            if (orderby.EndsWith(ascMark, true, CultureInfo.InvariantCulture))
            {
                orderby = orderby.Substring(0, orderby.Length - ascMark.Length);
                query.SortOrder = SortOrder.Ascending;
            }
            else if (orderby.EndsWith(descMark, true, CultureInfo.InvariantCulture))
            {
                orderby = orderby.Substring(0, orderby.Length - descMark.Length);
                query.SortOrder = SortOrder.Descending;
            }
            else
                query.SortOrder = SortOrder.Ascending;

            Type orderedType;
            if (query.GroupByExpression != null)
            {
                // When groupby is added to query, ordering will occur AFTER select, not before.
                orderedType = query.SelectExpression.ReturnType;
            }
            else
            {
                orderedType = query.OfType.Type;
            }

            query.OrderByExpression = parser.Parse(orderedType, orderby);
        }
#endif


        private void ParseOrderBy(PomonaQuery query, string s)
        {
            Type orderedType;
            if (query.GroupByExpression != null)
            {
                // When groupby is added to query, ordering will occur AFTER select, not before.
                orderedType = query.SelectExpression.ReturnType;
            }
            else
                orderedType = query.OfType.Type;
            query.OrderByExpressions = this.parser.ParseOrderBy(orderedType, s);
        }
    }
}