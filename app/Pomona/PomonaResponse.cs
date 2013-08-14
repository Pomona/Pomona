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
using Nancy;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaResponse
    {
        private readonly object entity;
        private readonly string expandedPaths;
        private readonly PomonaQuery query;
        private readonly IMappedType resultType;
        private readonly PomonaSession session;
        private readonly HttpStatusCode statusCode;

        public PomonaResponse(object entity, PomonaSession session, HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              IMappedType resultType = null)
        {
            if (session == null) throw new ArgumentNullException("session");
            this.entity = entity;
            this.session = session;
            this.statusCode = statusCode;
            this.expandedPaths = expandedPaths;
            this.resultType = resultType;
        }

        public PomonaResponse(PomonaQuery query, object entity, PomonaSession session)
            : this(query, entity, session, HttpStatusCode.OK)
        {
        }

        public PomonaResponse(PomonaQuery query, object entity, PomonaSession session, HttpStatusCode statusCode)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (session == null) throw new ArgumentNullException("session");
            this.query = query;
            this.entity = entity;
            this.session = session;
            this.statusCode = statusCode;
            expandedPaths = query.ExpandedPaths;
            resultType = query.ResultType;
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

        public PomonaSession Session
        {
            get { return session; }
        }

        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
        }
    }
}