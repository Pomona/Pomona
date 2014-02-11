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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Nancy.Testing;
using Pomona.Common.Internals;
using Pomona.Common.Web;
using HttpStatusCode = Pomona.Common.Web.HttpStatusCode;

namespace Pomona.TestHelpers
{
    public class NancyTestingWebClient : IWebClient
    {
        private readonly Browser browser;

        public NancyTestingWebClient(Browser browser)
        {
            if (browser == null) throw new ArgumentNullException("browser");
            this.browser = browser;
        }

        public NetworkCredential Credentials { get; set; }

        public WebClientResponseMessage Send(WebClientRequestMessage request)
        {
            Func<string, Action<BrowserContext>, BrowserResponse> browserMethod;

            switch (request.Method.ToUpper())
            {
                case "POST":
                    browserMethod = browser.Post;
                    break;
                case "PATCH":
                    browserMethod = browser.Patch;
                    break;
                case "GET":
                    browserMethod = browser.Get;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var uri = new Uri(request.Uri);
            var creds = Credentials;

            var browserResponse = browserMethod(request.Uri, bc =>
                {
                    bc.HttpRequest();
                    if (creds != null)
                    {
                        bc.BasicAuth(creds.UserName, creds.Password);
                    }
                    ((IBrowserContextValues) bc).QueryString = uri.Query;
                    foreach (var kvp in request.Headers)
                    {
                        foreach (var v in kvp.Value)
                            bc.Header(kvp.Key, v);
                    }
                    if (request.Data != null)
                    {
                        bc.Body(new MemoryStream(request.Data));
                    }
                });

            var responseHeaders = new HttpHeaders(
                browserResponse
                .Headers
                .Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value.WrapAsEnumerable())));

            if (browserResponse.Context.Response != null &&
                (!string.IsNullOrEmpty(browserResponse.Context.Response.ContentType)))
            {
                responseHeaders.Add("Content-Type", browserResponse.Context.Response.ContentType);
            }

            return new WebClientResponseMessage(request.Uri, browserResponse.Body.ToArray(),
                                                (HttpStatusCode) browserResponse.StatusCode,
                                                responseHeaders,
                                                "1.1");
        }

        public Task<WebClientResponseMessage> SendAsync(WebClientRequestMessage requestMessage)
        {
            throw new NotImplementedException();
        }
    }
}