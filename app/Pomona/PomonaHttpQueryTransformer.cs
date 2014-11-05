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
using System.Globalization;
using System.Linq;
using Nancy;
using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaHttpQueryTransformer : IHttpQueryTransformer
    {
        private readonly QueryExpressionParser parser;
        private readonly ITypeMapper typeMapper;


        public PomonaHttpQueryTransformer(ITypeMapper typeMapper, QueryExpressionParser parser)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (parser == null)
                throw new ArgumentNullException("parser");
            this.typeMapper = typeMapper;
            this.parser = parser;
        }

        #region IHttpQueryTransformer Members

        public PomonaQuery TransformRequest(PomonaRequest request, TransformedType rootType)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (rootType == null)
                throw new ArgumentNullException("rootType");

            TransformedType ofType = null;
            if (request.Query["$oftype"].HasValue)
            {
                ofType = (TransformedType) typeMapper.GetClassMapping((string) request.Query["$oftype"]);
            }

            var query = new PomonaQuery(rootType, ofType);

            if (request.Query["$debug"].HasValue)
            {
                query.DebugInfoKeys =
                    new HashSet<string>(((string) request.Query["$debug"]).ToLower().Split(',').Select(x => x.Trim()));
            }

            string filter = null;
            var top = 100;
            var skip = 0;

            if (request.Query["$totalcount"].HasValue && ((string) request.Query["$totalcount"]).ToLower() == "true")
                query.IncludeTotalCount = true;

            if (request.Query["$top"].HasValue)
                top = int.Parse(request.Query["$top"]);

            if (request.Query["$skip"].HasValue)
                skip = int.Parse(request.Query["$skip"]);

            if (request.Query["$filter"].HasValue)
                filter = (string) request.Query["$filter"];

            ParseFilterExpression(query, filter);
            var selectSourceType = query.OfType.Type;

            if (request.Query["$groupby"].HasValue)
            {
                var groupby = (string) request.Query["$groupby"];
                ParseGroupByExpression(query, groupby);
                selectSourceType =
                    typeof (IGrouping<,>).MakeGenericType(
                        query.GroupByExpression.ReturnType, selectSourceType);
            }

            if (request.Query["$projection"].HasValue)
            {
                var projectionString = (string) request.Query["$projection"];
                PomonaQuery.ProjectionType projection;
                if (!Enum.TryParse(projectionString, true, out projection))
                    throw new QueryParseException("\"" + projectionString +
                                                  "\" is not a valid value for query parameter $projection",
                        null,
                        QueryParseErrorReason.UnrecognizedProjection,
                        null);
                query.Projection = projection;
            }

            if (request.Query["$select"].HasValue)
            {
                var select = (string) request.Query["$select"];
                ParseSelect(query, select, selectSourceType);
            }

            if (request.Query["$orderby"].HasValue)
                ParseOrderBy(query, (string) request.Query["$orderby"]);

            query.Top = top;
            query.Skip = skip;

            if (request.Query["$expand"].HasValue)
            {
                // TODO: Translate expanded paths using TypeMapper
                query.ExpandedPaths = ((string) request.Query["$expand"]);
            }
            else
                query.ExpandedPaths = string.Empty;

            query.Url = request.Url;

            UpdateResultType(query);

            return query;
        }



        private void UpdateResultType(PomonaQuery query)
        {
            TypeSpec elementType = query.OfType;
            if (query.SelectExpression != null)
                elementType = typeMapper.GetClassMapping(query.SelectExpression.ReturnType);

            if (query.Projection == PomonaQuery.ProjectionType.First
                || query.Projection == PomonaQuery.ProjectionType.FirstOrDefault
                || query.Projection == PomonaQuery.ProjectionType.Single
                || query.Projection == PomonaQuery.ProjectionType.SingleOrDefault)
                query.ResultType = elementType;
        }


        private void ParseSelect(PomonaQuery query, string select, Type thisType)
        {
            query.SelectExpression = parser.ParseSelectList(thisType, select);
        }

        #endregion

        private void ParseFilterExpression(PomonaQuery query, string filter)
        {
            filter = filter ?? "true";
            query.FilterExpression = parser.Parse(query.OfType.Type, filter);
        }


        private void ParseGroupByExpression(PomonaQuery query, string groupby)
        {
            query.GroupByExpression = parser.ParseSelectList(query.OfType.Type, groupby);
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
            {
                orderedType = query.OfType.Type;
            }
            query.OrderByExpressions = parser.ParseOrderBy(orderedType, s);
        }
    }

}