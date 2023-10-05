#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

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
            if (context.Query["$oftype"].HasValue)
                ofType = (StructuredType)this.typeMapper.FromType((string)context.Query["$oftype"]);

            var query = new PomonaQuery(rootType, ofType);

            if (context.Query["$debug"].HasValue)
            {
                query.DebugInfoKeys =
                    new HashSet<string>(((string)context.Query["$debug"]).ToLower().Split(',').Select(x => x.Trim()));
            }

            string filter = null;
            var top = defaultTop ?? 100;
            var skip = 0;

            if (context.Query["$totalcount"].HasValue && ((string)context.Query["$totalcount"]).ToLower() == "true")
                query.IncludeTotalCount = true;

            if (context.Query["$top"].HasValue)
                top = int.Parse(context.Query["$top"]);

            if (context.Query["$skip"].HasValue)
                skip = int.Parse(context.Query["$skip"]);

            if (context.Query["$filter"].HasValue)
                filter = (string)context.Query["$filter"];

            ParseFilterExpression(query, filter);
            var selectSourceType = query.OfType.Type;

            if (context.Query["$groupby"].HasValue)
            {
                var groupby = (string)context.Query["$groupby"];
                ParseGroupByExpression(query, groupby);
                selectSourceType =
                    typeof(IGrouping<,>).MakeGenericType(
                        query.GroupByExpression.ReturnType, selectSourceType);
            }

            if (context.Query["$projection"].HasValue)
            {
                var projectionString = (string)context.Query["$projection"];
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

            if (context.Query["$select"].HasValue)
            {
                var select = (string)context.Query["$select"];
                ParseSelect(query, select, selectSourceType);
            }

            if (context.Query["$orderby"].HasValue)
                ParseOrderBy(query, (string)context.Query["$orderby"]);

            query.Top = top;
            query.Skip = skip;

            if (context.Query["$expand"].HasValue)
            {
                // TODO: Translate expanded paths using TypeMapper
                query.ExpandedPaths = ((string)context.Query["$expand"]);
            }
            else
                query.ExpandedPaths = string.Empty;

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
