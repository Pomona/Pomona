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
using System.Runtime.Serialization;
using Nancy;

namespace Pomona
{
    [Serializable]
    public class PomonaException : ApplicationException
    {
        private readonly List<KeyValuePair<string, string>> responseHeaders;
        private readonly HttpStatusCode statusCode;
        private readonly object entity;

        public PomonaException()
        {
        }

        public PomonaException(string message,object entity=null) : base(message)
        {
            this.entity = entity;
            statusCode = HttpStatusCode.InternalServerError;
        }

        public PomonaException(string message, Exception innerException, object entity = null)
            : base(message, innerException)
        {
            this.entity = entity;
            statusCode = HttpStatusCode.InternalServerError;
        }

        public PomonaException(string message, Exception innerException, HttpStatusCode statusCode,
                               IEnumerable<KeyValuePair<string, string>> responseHeaders = null, object entity = null)
            : base(message, innerException)
        {
            this.entity = entity;
            if (responseHeaders != null)
                this.responseHeaders = responseHeaders.ToList();

            this.statusCode = statusCode;
        }

        protected PomonaException(SerializationInfo info, StreamingContext context, object entity = null)
            : base(info, context)
        {
            this.entity = entity;
        }

        public List<KeyValuePair<string, string>> ResponseHeaders
        {
            get { return responseHeaders; }
        }

        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
        }

        public object Entity {
            get { return entity; }
        }
    }
}