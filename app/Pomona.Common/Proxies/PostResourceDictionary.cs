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

namespace Pomona.Common.Proxies
{
    internal class PostResourceDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly PostResourceBase owner;
        private readonly string propertyName;
        private readonly IDictionary<TKey, TValue> wrapped = new Dictionary<TKey, TValue>();

        internal PostResourceDictionary(PostResourceBase owner, string propertyName)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            this.owner = owner;
            this.propertyName = propertyName;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return wrapped.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            wrapped.Add(item);
            SetDirty();
        }

        public void Clear()
        {
            wrapped.Clear();
            SetDirty();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return wrapped.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            wrapped.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            SetDirty();
            return wrapped.Remove(item);
        }

        public int Count
        {
            get { return wrapped.Count; }
        }

        public bool IsReadOnly
        {
            get { return wrapped.IsReadOnly; }
        }

        public bool ContainsKey(TKey key)
        {
            return wrapped.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            wrapped.Add(key, value);
            SetDirty();
        }

        public bool Remove(TKey key)
        {
            SetDirty();
            return wrapped.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return wrapped.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return wrapped[key]; }
            set
            {
                wrapped[key] = value;
                SetDirty();
            }
        }

        public ICollection<TKey> Keys
        {
            get { return wrapped.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return wrapped.Values; }
        }

        private void SetDirty()
        {
            owner.SetDirty(propertyName);
        }
    }
}