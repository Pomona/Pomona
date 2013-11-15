#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaResponse
    {
        internal static readonly object NoBodyEntity = new object();

        private readonly object entity;
        private readonly string expandedPaths;
        private readonly List<KeyValuePair<string, string>> responseHeaders;
        private readonly IMappedType resultType;
        private readonly HttpStatusCode statusCode;

        public PomonaResponse(object entity, HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              IMappedType resultType = null,
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
        {
            this.entity = entity;
            this.statusCode = statusCode;
            this.expandedPaths = expandedPaths;
            this.resultType = resultType;

            if (responseHeaders != null)
                this.responseHeaders = responseHeaders.ToList();
        }

        public PomonaResponse(PomonaQuery query, object entity)
            : this(query, entity, HttpStatusCode.OK)
        {
        }

        public PomonaResponse(PomonaQuery query, object entity, HttpStatusCode statusCode)
        {
            if (query == null) throw new ArgumentNullException("query");
            this.entity = entity;
            this.statusCode = statusCode;
            expandedPaths = query.ExpandedPaths;
            resultType = query.ResultType;
        }

        public List<KeyValuePair<string, string>> ResponseHeaders
        {
            get { return responseHeaders; }
        }

        public IMappedType ResultType
        {
            get { return resultType; }
        }

        public string ExpandedPaths
        {
            get { return expandedPaths; }
        }

        public object Entity
        {
            get { return entity; }
        }

        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
        }
    }
}