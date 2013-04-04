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

namespace Pomona.Common.Web
{
    public class WrappedWebClient : IWebClient
    {
        private readonly IDictionary<string, string> headers;
        private readonly WebClient webClient = new WebClient();

        public WrappedWebClient()
        {
            headers = new HeaderDictionaryWrapper(webClient);
        }

        public IDictionary<string, string> Headers
        {
            get { return headers; }
        }

        public byte[] DownloadData(string uri)
        {
            return webClient.DownloadData(uri);
        }

        public byte[] UploadData(string uri, string httpMethod, byte[] requestBytes)
        {
            return webClient.UploadData(uri, httpMethod, requestBytes);
        }

        private class HeaderDictionaryWrapper : IDictionary<string, string>
        {
            private readonly WebClient webClient;

            public HeaderDictionaryWrapper(WebClient webClient)
            {
                if (webClient == null) throw new ArgumentNullException("webClient");
                this.webClient = webClient;
            }

            private WebHeaderCollection Headers
            {
                get { return webClient.Headers; }
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