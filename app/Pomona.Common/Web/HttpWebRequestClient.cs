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
using System.Threading.Tasks;
using Pomona.Common.Internals;

namespace Pomona.Common.Web
{
    public class HttpWebRequestClient : IWebClient
    {
        private readonly HeaderDictionaryWrapper headers = new HeaderDictionaryWrapper(new WebHeaderCollection());

        public IDictionary<string, string> Headers
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
                    webRequest.Headers.Add(h.Key, h.Value);
                else
                {
                    switch (h.Key)
                    {
                        case "Accept":
                            webRequest.Accept = h.Value;
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
                                                    new HeaderDictionaryWrapper(webResponse.Headers),
                                                    webResponse.ProtocolVersion.ToString());
            }
        }

        public Task<WebClientResponseMessage> SendAsync(WebClientRequestMessage requestMessage)
        {
            throw new NotSupportedException("Does not support async. Use WrappedHttpClient instead.");
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

        private class HeaderDictionaryWrapper : IDictionary<string, string>
        {
            private readonly WebHeaderCollection headers;

            public HeaderDictionaryWrapper(WebHeaderCollection headers)
            {
                if (headers == null) throw new ArgumentNullException("headers");
                this.headers = headers;
            }

            internal WebHeaderCollection Headers
            {
                get { return headers; }
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                for (var i = 0; i < Headers.Count; i++)
                {
                    yield return new KeyValuePair<string, string>(Headers.GetKey(i), Headers.Get(i));
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(KeyValuePair<string, string> item)
            {
                Headers.Add(item.Key, item.Value);
            }

            public void Clear()
            {
                Headers.Clear();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                // Returns null if it has no value.
                return Headers.Get(item.Key) == item.Value;
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                this.ToList().CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                if (!Contains(item))
                    return false;

                Headers.Remove(item.Key);
                return true;
            }

            public int Count
            {
                get { return Headers.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool ContainsKey(string key)
            {
                return Headers.Get(key) != null;
            }

            public void Add(string key, string value)
            {
                Headers.Add(key, value);
            }

            public bool Remove(string key)
            {
                if (Headers.Get(key) == null)
                    return false;
                Headers.Remove(key);
                return true;
            }

            public bool TryGetValue(string key, out string value)
            {
                value = Headers.Get(key);
                return value != null;
            }

            public string this[string key]
            {
                get
                {
                    string value;
                    if (!TryGetValue(key, out value))
                    {
                        throw new KeyNotFoundException();
                    }
                    return value;
                }
                set { Headers.Set(key, value); }
            }

            public ICollection<string> Keys
            {
                get { return this.Select(x => x.Key).ToList(); }
            }

            public ICollection<string> Values
            {
                get { return this.Select(x => x.Value).ToList(); }
            }
        }
    }
}