#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

using Newtonsoft.Json;

namespace Pomona.Common.Web
{
    [JsonConverter(typeof(HttpHeadersConverter))]
    public class HttpHeaders : IDictionary<string, IList<string>>, ICloneable
    {
        private static readonly StringComparer keyComparer = StringComparer.OrdinalIgnoreCase;

        private readonly IDictionary<string, IList<string>> dict =
            new Dictionary<string, IList<string>>(keyComparer);


        public HttpHeaders()
        {
            this.dict = new Dictionary<string, IList<string>>();
        }


        public HttpHeaders(HttpHeaders copiedHeaders)
        {
            if (copiedHeaders == null)
                throw new ArgumentNullException("copiedHeaders");
            this.dict = copiedHeaders.dict.ToDictionary(x => x.Key, x => (IList<string>)new List<string>(x.Value), keyComparer);
        }


        public HttpHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source)
        {
            this.dict = source.GroupBy(x => x.Key)
                              .ToDictionary(x => x.Key, x => (IList<string>)x.SelectMany(y => y.Value).ToList(), keyComparer);
        }


        public string ContentType
        {
            get { return GetSingle("Content-Type"); }
            set { SetSingle("Content-Type", value); }
        }

        /// <summary>
        /// MediaType from Content-Type header
        /// </summary>
        public string MediaType
        {
            get { return ContentType != null ? new ContentType(ContentType).MediaType : null; }
        }


        public void Add(string key, string value)
        {
            IList<string> list;
            if (!this.dict.TryGetValue(key, out list))
            {
                list = new List<string>();
                this.dict[key] = list;
            }
            list.Add(value);
        }


        public IEnumerable<KeyValuePair<string, string>> GetExpanded()
        {
            return this.dict.SelectMany(x => x.Value, (k, v) => new KeyValuePair<string, string>(k.Key, v));
        }


        public string GetFirstOrDefault(string key)
        {
            IList<string> list;
            if (!this.dict.TryGetValue(key, out list))
                return null;
            return list.FirstOrDefault();
        }


        public IEnumerable<string> GetValues(string key)
        {
            return this.dict.SafeGet(key) ?? Enumerable.Empty<string>();
        }


        private string GetSingle(string key)
        {
            return GetFirstOrDefault(key);
        }


        private void SetSingle(string key, string value)
        {
            if (value != null)
                this.dict[key] = new List<string>() { value };
            else
                this.dict.Remove(key);
        }


        public void Add(KeyValuePair<string, IList<string>> item)
        {
            this.dict.Add(item);
        }


        public void Add(string key, IList<string> value)
        {
            this.dict.Add(key, value);
        }


        public void Clear()
        {
            this.dict.Clear();
        }


        public object Clone()
        {
            return new HttpHeaders(this);
        }


        public bool Contains(KeyValuePair<string, IList<string>> item)
        {
            return this.dict.Contains(item);
        }


        public bool ContainsKey(string key)
        {
            return this.dict.ContainsKey(key);
        }


        public void CopyTo(KeyValuePair<string, IList<string>>[] array, int arrayIndex)
        {
            this.dict.CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return this.dict.Count; }
        }


        public IEnumerator<KeyValuePair<string, IList<string>>> GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }


        public bool IsReadOnly
        {
            get { return this.dict.IsReadOnly; }
        }

        public IList<string> this[string key]
        {
            get { return this.dict[key]; }
            set { this.dict[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return this.dict.Keys; }
        }


        public bool Remove(KeyValuePair<string, IList<string>> item)
        {
            return this.dict.Remove(item);
        }


        public bool Remove(string key)
        {
            return this.dict.Remove(key);
        }


        public bool TryGetValue(string key, out IList<string> value)
        {
            return this.dict.TryGetValue(key, out value);
        }


        public ICollection<IList<string>> Values
        {
            get { return this.dict.Values; }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}