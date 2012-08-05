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
using System.Linq;
using Nancy;

namespace Pomona
{
    public class PomonaQueryTransformer : IHttpQueryTransformer
    {
        private readonly PomonaQueryFilterParser filterParser;
        private readonly TypeMapper typeMapper;

        public PomonaQueryTransformer(TypeMapper typeMapper, PomonaQueryFilterParser filterParser)
        {
            if (typeMapper == null) throw new ArgumentNullException("typeMapper");
            if (filterParser == null) throw new ArgumentNullException("filterParser");
            this.typeMapper = typeMapper;
            this.filterParser = filterParser;
        }

        #region IHttpQueryTransformer Members

        public IPomonaQuery TransformRequest(Request request, NancyContext nancyContext, TransformedType rootType)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (nancyContext == null) throw new ArgumentNullException("nancyContext");
            if (rootType == null) throw new ArgumentNullException("rootType");

            var query = new PomonaQuery(rootType);

            var filter = (string) request.Query.filter;

            bool errorWhileParsing;

            query.Conditions = filterParser.Parse(rootType, filter, out errorWhileParsing).ToList();

            // TODO: Translate expanded paths using TypeMapper
            query.ExpandedPaths = ((string) request.Query.expand).Split(new[] {','},
                                                                        StringSplitOptions.RemoveEmptyEntries);

            return query;
        }

        #endregion
    }
}