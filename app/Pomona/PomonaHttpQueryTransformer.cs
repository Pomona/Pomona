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
    public class PomonaHttpQueryTransformer
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

        public PomonaQuery TransformRequest(PomonaContext context, ComplexType rootType, int? defaultTop = null)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (rootType == null)
                throw new ArgumentNullException("rootType");

            ComplexType ofType = null;
            if (context.Query["$oftype"].HasValue)
            {
                ofType = (ComplexType) typeMapper.GetClassMapping((string) context.Query["$oftype"]);
            }

            var query = new PomonaQuery(rootType, ofType);

            if (context.Query["$debug"].HasValue)
            {
                query.DebugInfoKeys =
                    new HashSet<string>(((string) context.Query["$debug"]).ToLower().Split(',').Select(x => x.Trim()));
            }

            string filter = null;
            var top = defaultTop ?? 100;
            var skip = 0;

            if (context.Query["$totalcount"].HasValue && ((string) context.Query["$totalcount"]).ToLower() == "true")
                query.IncludeTotalCount = true;

            if (context.Query["$top"].HasValue)
                top = int.Parse(context.Query["$top"]);

            if (context.Query["$skip"].HasValue)
                skip = int.Parse(context.Query["$skip"]);

            if (context.Query["$filter"].HasValue)
                filter = (string) context.Query["$filter"];

            ParseFilterExpression(query, filter);
            var selectSourceType = query.OfType.Type;

            if (context.Query["$groupby"].HasValue)
            {
                var groupby = (string) context.Query["$groupby"];
                ParseGroupByExpression(query, groupby);
                selectSourceType =
                    typeof (IGrouping<,>).MakeGenericType(
                        query.GroupByExpression.ReturnType, selectSourceType);
            }

            if (context.Query["$projection"].HasValue)
            {
                var projectionString = (string) context.Query["$projection"];
                PomonaQuery.ProjectionType projection;
                if (!Enum.TryParse(projectionString, true, out projection))
                    throw new QueryParseException("\"" + projectionString +
                                                  "\" is not a valid value for query parameter $projection",
                        null,
                        QueryParseErrorReason.UnrecognizedProjection,
                        null);
                query.Projection = projection;
            }

            if (context.Query["$select"].HasValue)
            {
                var select = (string) context.Query["$select"];
                ParseSelect(query, select, selectSourceType);
            }

            if (context.Query["$orderby"].HasValue)
                ParseOrderBy(query, (string) context.Query["$orderby"]);

            query.Top = top;
            query.Skip = skip;

            if (context.Query["$expand"].HasValue)
            {
                // TODO: Translate expanded paths using TypeMapper
                query.ExpandedPaths = ((string) context.Query["$expand"]);
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