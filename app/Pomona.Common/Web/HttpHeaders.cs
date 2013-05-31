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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.Web
{
    public class HttpHeaders : IHttpHeaders
    {
        private readonly IDictionary<string, IList<string>> dict = new Dictionary<string, IList<string>>();

        public HttpHeaders()
        {
            dict = new Dictionary<string, IList<string>>();
        }

        public HttpHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source)
        {
            dict = source.GroupBy(x => x.Key)
                         .ToDictionary(x => x.Key, x => (IList<string>) x.SelectMany(y => y.Value).ToList());
        }

        public IEnumerator<KeyValuePair<string, IList<string>>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, IList<string>> item)
        {
            dict.Add(item);
        }

        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(KeyValuePair<string, IList<string>> item)
        {
            return dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IList<string>>[] array, int arrayIndex)
        {
            dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, IList<string>> item)
        {
            return dict.Remove(item);
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return dict.IsReadOnly; }
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public void Add(string key, IList<string> value)
        {
            dict.Add(key, value);
        }

        public bool Remove(string key)
        {
            return dict.Remove(key);
        }

        public bool TryGetValue(string key, out IList<string> value)
        {
            return dict.TryGetValue(key, out value);
        }

        public IList<string> this[string key]
        {
            get { return dict[key]; }
            set { dict[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return dict.Keys; }
        }

        public ICollection<IList<string>> Values
        {
            get { return dict.Values; }
        }

        public void Add(string key, string value)
        {
            IList<string> list;
            if (!dict.TryGetValue(key, out list))
            {
                list = new List<string>();
                dict[key] = list;
            }
            list.Add(value);
        }

        public string GetFirst(string key, string value)
        {
            return dict[key].First();
        }

        public IEnumerable<string> GetValues(string key)
        {
            return dict.SafeGet(key) ?? Enumerable.Empty<string>();
        }
    }
}