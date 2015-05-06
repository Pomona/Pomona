#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.IO;

using Nancy;

using Pomona.Common;

namespace Pomona
{
    public class PomonaRequest
    {
        private readonly HttpMethod method;
        private readonly string relativePath;
        private readonly string url;
        private RequestHeaders headers;


        public PomonaRequest(string url,
                             string relativePath,
                             HttpMethod method = HttpMethod.Get,
                             RequestHeaders headers = null,
                             Stream body = null,
                             DynamicDictionary query = null)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            this.method = method;
            this.url = url;
            this.relativePath = relativePath;
            Body = body;
            this.headers = headers;
            Query = query ?? new DynamicDictionary();
        }


        public Stream Body { get; internal set; }

        public RequestHeaders Headers
        {
            get { return this.headers ?? (this.headers = new RequestHeaders(new Dictionary<string, IEnumerable<string>>())); }
        }

        public HttpMethod Method
        {
            get { return this.method; }
        }

        public DynamicDictionary Query { get; internal set; }

        public string RelativePath
        {
            get { return this.relativePath; }
        }

        public string Url
        {
            get { return this.url; }
        }
    }
}