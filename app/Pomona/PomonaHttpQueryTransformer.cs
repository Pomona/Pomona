#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright � 2012 Karsten Nikolai Strand
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
using System.Globalization;
using System.Linq;

using Nancy;

using Pomona.Common;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaHttpQueryTransformer : IHttpQueryTransformer
    {
        private readonly QueryExpressionParser parser;
        private readonly TypeMapper typeMapper;


        public PomonaHttpQueryTransformer(TypeMapper typeMapper, QueryExpressionParser parser)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (parser == null)
                throw new ArgumentNullException("parser");
            this.typeMapper = typeMapper;
            this.parser = parser;
        }

        #region IHttpQueryTransformer Members

        public IPomonaQuery TransformRequest(Request request, NancyContext nancyContext, TransformedType rootType)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (nancyContext == null)
                throw new ArgumentNullException("nancyContext");
            if (rootType == null)
                throw new ArgumentNullException("rootType");

            if (request.Query["$oftype"].HasValue)
            {
                rootType = (TransformedType)typeMapper.GetClassMapping((string)request.Query["$oftype"]);
            }

            var query = new PomonaQuery(rootType);

            string filter = null;
            var top = 10;
            var skip = 0;

            if (request.Query["$totalcount"].HasValue && ((string) request.Query["$totalcount"]).ToLower() == "true")
                query.IncludeTotalCount = true;

            if (request.Query["$top"].HasValue)
                top = int.Parse(request.Query["$top"]);

            if (request.Query["$skip"].HasValue)
                skip = int.Parse(request.Query["$skip"]);

            if (request.Query["$filter"].HasValue)
                filter = (string)request.Query["$filter"];

            ParseFilterExpression(query, filter);
            var selectSourceType = query.TargetType.MappedTypeInstance;

            if (request.Query["$groupby"].HasValue)
            {
                var groupby = (string)request.Query["$groupby"];
                ParseGroupByExpression(query, groupby);
                selectSourceType =
                    typeof(IGrouping<,>).MakeGenericType(
                            query.GroupByExpression.ReturnType, selectSourceType);
            }

            if (request.Query["$select"].HasValue)
            {
                string select = (string)request.Query["$select"];
                ParseSelect(query, select, selectSourceType);
            }

            if (request.Query["$orderby"].HasValue)
                ParseOrderBy(query, (string)request.Query["$orderby"]);

            query.Top = top;
            query.Skip = skip;

            if (request.Query["$expand"].HasValue)
            {
                // TODO: Translate expanded paths using TypeMapper
                query.ExpandedPaths = ((string)request.Query["$expand"]);
            }
            else
                query.ExpandedPaths = string.Empty;

            query.Url = request.Url.ToString();

            return query;
        }


        private void ParseSelect(PomonaQuery query, string select, Type thisType)
        {
            query.SelectExpression = this.parser.ParseSelectList(thisType, select);
        }

        #endregion

        private void ParseFilterExpression(PomonaQuery query, string filter)
        {
            filter = filter ?? "true";
            query.FilterExpression = this.parser.Parse(query.TargetType.MappedTypeInstance, filter);
        }


        private void ParseGroupByExpression(PomonaQuery query, string groupby)
        {
            query.GroupByExpression = this.parser.Parse(query.TargetType.MappedTypeInstance, groupby);
        }


        private void ParseOrderBy(PomonaQuery query, string orderby)
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
                orderedType = query.TargetType.MappedTypeInstance;
            }

            query.OrderByExpression = this.parser.Parse(orderedType, orderby);
        }
    }
}