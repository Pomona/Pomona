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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pomona.Common.Web
{
    public class WrappedHttpClient : IWebClient
    {
        private readonly HttpClient httpClient;
        private readonly HttpHeaders headers;

        public WrappedHttpClient()
        {
            httpClient = new HttpClient();
            headers = new HttpHeaders();
        }

        public IHttpHeaders Headers
        {
            get { return headers; }
        }

        public NetworkCredential Credentials { get; set; }

        public WebClientResponseMessage Send(WebClientRequestMessage request)
        {
            return SendAsync(request).Result;
        }

        public async Task<WebClientResponseMessage> SendAsync(WebClientRequestMessage requestMessage)
        {
            var httpRequestMessage = new HttpRequestMessage(new System.Net.Http.HttpMethod(requestMessage.Method), requestMessage.Uri);

            if (Credentials != null)
                throw new NotImplementedException("Authentication not yet implemented in async web client!");

            if (requestMessage.Data != null)
                httpRequestMessage.Content = new ByteArrayContent(requestMessage.Data);

            foreach (var header in Headers.Concat(requestMessage.Headers))
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            var responseData = await httpResponseMessage.Content.ReadAsByteArrayAsync();

            return new WebClientResponseMessage(requestMessage.Uri, responseData,
                                                (HttpStatusCode) httpResponseMessage.StatusCode,
                                                new HttpHeaders(httpResponseMessage.Headers), 
                                                httpResponseMessage.Version.ToString());
        }
    }
}