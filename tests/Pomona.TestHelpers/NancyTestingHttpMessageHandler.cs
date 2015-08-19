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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Nancy;
using Nancy.IO;

using Pomona.Common.Internals;
using Pomona.Common.Web;

using HttpStatusCode = System.Net.HttpStatusCode;

namespace Pomona.TestHelpers
{
    public class NancyTestingHttpMessageHandler : HttpMessageHandler
    {
        private static readonly HashSet<string> contentHeaders = new HashSet<string>()
        {
            "Allow",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            "Content-Type",
            "Expires",
            "Last-Modified"
        };

        private readonly INancyEngine engine;

        public NancyTestingHttpMessageHandler(INancyEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            this.engine = engine;
        }


        private async Task<Request> MapNancyRequest(HttpRequestMessage request)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headersToCopy = request.Headers;
            RequestStream requestStream = null;
            if (request.Content != null)
            {
                headersToCopy = headersToCopy.Concat(request.Content.Headers);
                requestStream = new RequestStream(await request.Content.ReadAsStreamAsync(),
                                                  request.Content.Headers.ContentLength.GetValueOrDefault(), true);
            }

            if (Credentials != null)
            {
                string encodedCredentials =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", Credentials.UserName, Credentials.Password)));
                headersToCopy =
                    headersToCopy.Append(new KeyValuePair<string, IEnumerable<string>>("Authorization",
                                                                                       new[] { "Basic " + encodedCredentials }));
            }

            var nancyRequest = new Request(request.Method.ToString(), request.RequestUri,
                                           requestStream, headersToCopy.ToDictionary(x => x.Key, x => x.Value));
            return nancyRequest;
        }


        private HttpResponseMessage MapNancyResponse(Response src)
        {
            var dst = new HttpResponseMessage((HttpStatusCode)src.StatusCode);
            if (src.Contents != null)
            {
                using (var memStream = new MemoryStream())
                {
                    src.Contents(memStream);
                    dst.Content = new ByteArrayContent(memStream.ToArray());
                }
                foreach (var header in src.Headers)
                {
                    if (contentHeaders.Contains(header.Key))
                        dst.Content.Headers.Add(header.Key, header.Value);
                }
                if (src.ContentType != null)
                    dst.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(src.ContentType);
            }
            foreach (var header in src.Headers.Where(x => !contentHeaders.Contains(x.Key)))
                dst.Headers.Add(header.Key, header.Value);
            return dst;
        }


        public NetworkCredential Credentials { get; set; }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var nancyRequest = await MapNancyRequest(request);

            var context = await this.engine.HandleRequest(nancyRequest, ctx => ctx, cancellationToken);

            return MapNancyResponse(context.Response);
        }


        public void Dispose()
        {
        }
    }
}