#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Globalization;
using System.Linq;

using Nancy;

using Pomona.Client;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaQueryTransformer : IHttpQueryTransformer
    {
        private readonly QueryFilterExpressionParser filterParser;
        private readonly TypeMapper typeMapper;


        public PomonaQueryTransformer(TypeMapper typeMapper, QueryFilterExpressionParser filterParser)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (filterParser == null)
                throw new ArgumentNullException("filterParser");
            this.typeMapper = typeMapper;
            this.filterParser = filterParser;
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

            var query = new PomonaQuery(rootType);

            string select = null;
            string filter = null;
            var top = 10;
            var skip = 0;

            if (request.Query.top.HasValue)
                top = int.Parse(request.Query.top);

            if (request.Query.skip.HasValue)
                skip = int.Parse(request.Query.skip);

            if (request.Query.filter.HasValue)
                filter = (string)request.Query.filter;

            if (request.Query.select.HasValue)
                select = (string)request.Query.select;

            if (request.Query.orderby.HasValue)
                ParseOrderBy(query, (string)request.Query.orderby);

            ParseFilterExpression(query, filter);

            query.Top = top;
            query.Skip = skip;

            if (request.Query.expand.HasValue)
            {
                // TODO: Translate expanded paths using TypeMapper
                query.ExpandedPaths = ((string)request.Query.expand)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct()
                    .Select(x => this.typeMapper.ConvertToInternalPropertyPath(rootType, x))
                    .ToList();
            }
            else
                query.ExpandedPaths = Enumerable.Empty<string>();

            query.Url = request.Url;

            return query;
        }

        #endregion

        private void ParseFilterExpression(PomonaQuery query, string filter)
        {
            filter = filter ?? "true";
            query.FilterExpression = this.filterParser.Parse(query.TargetType.SourceType, filter);
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

            query.OrderByExpression = this.filterParser.Parse(query.TargetType.SourceType, orderby);
        }
    }
}