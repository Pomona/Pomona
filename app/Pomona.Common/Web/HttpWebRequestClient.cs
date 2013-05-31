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
using System.Linq;
using System.Net;
using Pomona.Common.Internals;

namespace Pomona.Common.Web
{
    public class HttpWebRequestClient : IWebClient
    {
        private readonly IHttpHeaders headers = new HttpHeaders();

        public IHttpHeaders Headers
        {
            get { return headers; }
        }

        public NetworkCredential Credentials { get; set; }

        public WebClientResponseMessage Send(WebClientRequestMessage request)
        {
            var webRequest = (HttpWebRequest) WebRequest.Create(request.Uri);

            if (Credentials != null)
                webRequest.Credentials = Credentials;

            foreach (var h in headers.Concat(request.Headers))
            {
                if (!WebHeaderCollection.IsRestricted(h.Key))
                {
                    foreach (var v in h.Value)
                    {
                        webRequest.Headers.Add(h.Key, v);
                    }
                }
                else
                {
                    switch (h.Key)
                    {
                        case "Accept":
                            webRequest.Accept = h.Value.Single();
                            break;

                        default:
                            throw new NotImplementedException("Setting restricted header " + h.Key + " not implemented.");
                    }
                }
            }

            webRequest.ProtocolVersion = Version.Parse(request.ProtocolVersion);
            webRequest.Method = request.Method;

            if (request.Data != null)
            {
                webRequest.ContentLength = request.Data.Length;
                using (var requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(request.Data, 0, request.Data.Length);
                }
            }

            Exception innerException;
            using (var webResponse = GetResponseNoThrow(webRequest, out innerException))
            using (var responseStream = webResponse.GetResponseStream())
            {
                var responseBytes = responseStream.ReadAllBytes();

                return new WebClientResponseMessage(webResponse.ResponseUri.ToString(), responseBytes,
                                                    (HttpStatusCode) webResponse.StatusCode,
                                                    new HttpHeaders(ConvertHeaders(webResponse.Headers)),
                                                    webResponse.ProtocolVersion.ToString());
            }
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ConvertHeaders(
            WebHeaderCollection webHeaders)
        {
            for (var i = 0; i < webHeaders.Count; i++)
            {
                var key = webHeaders.GetKey(i);
                yield return new KeyValuePair<string, IEnumerable<string>>(key, webHeaders.GetValues(i));
            }
        }

        private static HttpWebResponse GetResponseNoThrow(HttpWebRequest request, out Exception thrownException)
        {
            try
            {
                thrownException = null;
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException ex)
            {
                thrownException = ex;
                return (HttpWebResponse) ex.Response;
            }
        }
    }
}