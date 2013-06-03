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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomona.Common.Web
{
    public class WebClientResponseMessage
    {
        private readonly byte[] data;
        private readonly IHttpHeaders headers;
        private readonly string protocolVersion;
        private readonly HttpStatusCode statusCode;
        private readonly string uri;

        public WebClientResponseMessage(string uri, byte[] data, HttpStatusCode statusCode,
                                        IHttpHeaders headers, string protocolVersion)
        {
            this.headers = headers;
            this.uri = uri;
            this.data = data;
            this.statusCode = statusCode;
            this.protocolVersion = protocolVersion;
        }

        public IHttpHeaders Headers
        {
            get { return headers; }
        }

        public string Uri
        {
            get { return uri; }
        }

        public byte[] Data
        {
            get { return data; }
        }

        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("HTTP/{0} {1} {2}\r\n", protocolVersion, (int) statusCode, statusCode);
            foreach (var h in headers)
            {
                foreach (var v in h.Value)
                    sb.AppendFormat("{0}: {1}\r\n", h.Key, v);
            }
            sb.AppendLine();

            if (data != null)
            {
                sb.Append(Encoding.UTF8.GetString(data));
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}