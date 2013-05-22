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
        private readonly IDictionary<string, string> headers = new Dictionary<string, string>();
        private readonly HttpClient httpClient;

        public WrappedHttpClient()
        {
            httpClient = new HttpClient();
            headers = new HeadersDictionaryWrapper(httpClient.DefaultRequestHeaders);
        }

        public IDictionary<string, string> Headers
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
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod(requestMessage.Method), requestMessage.Uri);

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
                                                httpResponseMessage.Headers.Select(
                                                    x =>
                                                    new KeyValuePair<string, string>(x.Key, string.Join(",", x.Value))),
                                                httpResponseMessage.Version.ToString());
        }

        private class HeadersDictionaryWrapper : IDictionary<string, string>
        {
            private readonly HttpHeaders headers;

            public HeadersDictionaryWrapper(HttpHeaders headers)
            {
                this.headers = headers;
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return
                    headers.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.FirstOrDefault()))
                           .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(KeyValuePair<string, string> item)
            {
                headers.Add(item.Key, item.Value);
            }

            public void Clear()
            {
                headers.Clear();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                IEnumerable<string> values;
                if (!headers.TryGetValues(item.Key, out values))
                    return false;
                return values.Contains(item.Value);
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                this.ToList().CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                if (Contains(item))
                {
                    headers.Remove(item.Value);
                    return true;
                }
                return false;
            }

            public int Count
            {
                get { return headers.Count(); }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool ContainsKey(string key)
            {
                return headers.Contains(key);
            }

            public void Add(string key, string value)
            {
                headers.Add(key, value);
            }

            public bool Remove(string key)
            {
                return headers.Remove(key);
            }

            public bool TryGetValue(string key, out string value)
            {
                IEnumerable<string> values;
                if (headers.TryGetValues(key, out values))
                {
                    value = string.Join(",", values);
                    return true;
                }
                value = null;
                return false;
            }

            public string this[string key]
            {
                get { return string.Join(",", headers.GetValues(key)); }
                set
                {
                    headers.Remove(key);
                    headers.Add(key, value);
                }
            }

            public ICollection<string> Keys
            {
                get { return headers.Select(x => x.Key).ToList(); }
            }

            public ICollection<string> Values
            {
                get
                {
                    return
                        headers.Select(x => string.Join(",", x.Value)).ToList();
                }
            }
        }
    }
}