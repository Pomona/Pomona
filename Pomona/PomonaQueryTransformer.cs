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

using System;
using System.Linq.Expressions;
using System.Reflection;
using Nancy;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaQueryTransformer : IHttpQueryTransformer
    {
        private static MethodInfo toExpressionGenericMethod;
        private readonly QueryFilterExpressionParser filterParser;
        private readonly TypeMapper typeMapper;

        static PomonaQueryTransformer()
        {
            toExpressionGenericMethod = typeof (PomonaQueryTransformer).GetMethod(
                "ToExpressionGeneric", BindingFlags.Instance | BindingFlags.NonPublic);
        }

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

            string filter = null;
            var top = 10;
            var skip = 0;

            if (request.Query.top.HasValue)
            {
                top = int.Parse(request.Query.top);
            }

            if (request.Query.skip.HasValue)
            {
                skip = int.Parse(request.Query.skip);
            }

            if (request.Query.filter.HasValue)
            {
                filter = (string) request.Query.filter;
            }

            var sourceType = rootType.SourceType;
            var parseMethod = toExpressionGenericMethod.MakeGenericMethod(sourceType);
            query.FilterExpression = (Expression) parseMethod.Invoke(this, new[] {filter});

            query.Top = top;
            query.Skip = skip;

            // TODO: Translate expanded paths using TypeMapper
            query.ExpandedPaths = ((string) request.Query.expand).Split(
                new[] {','},
                StringSplitOptions.RemoveEmptyEntries);

            query.Url = request.Url;

            return query;
        }

        #endregion

        private Expression ToExpressionGeneric<T>(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                Expression<Func<T, bool>> trueExpr = x => true;
                return trueExpr;
            }
            return filterParser.Parse<T>(filter);
        }
    }
}