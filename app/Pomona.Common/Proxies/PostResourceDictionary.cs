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
            if (owner == null)
                throw new ArgumentNullException("owner");
            this.owner = owner;
            this.propertyName = propertyName;
        }


        private void SetDirty()
        {
            this.owner.SetDirty(this.propertyName);
        }


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.wrapped.Add(item);
            SetDirty();
        }


        public void Add(TKey key, TValue value)
        {
            this.wrapped.Add(key, value);
            SetDirty();
        }


        public void Clear()
        {
            this.wrapped.Clear();
            SetDirty();
        }


        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.wrapped.Contains(item);
        }


        public bool ContainsKey(TKey key)
        {
            return this.wrapped.ContainsKey(key);
        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.wrapped.CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return this.wrapped.Count; }
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.wrapped.GetEnumerator();
        }


        public bool IsReadOnly
        {
            get { return this.wrapped.IsReadOnly; }
        }

        public TValue this[TKey key]
        {
            get { return this.wrapped[key]; }
            set
            {
                this.wrapped[key] = value;
                SetDirty();
            }
        }

        public ICollection<TKey> Keys
        {
            get { return this.wrapped.Keys; }
        }


        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            SetDirty();
            return this.wrapped.Remove(item);
        }


        public bool Remove(TKey key)
        {
            SetDirty();
            return this.wrapped.Remove(key);
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.wrapped.TryGetValue(key, out value);
        }


        public ICollection<TValue> Values
        {
            get { return this.wrapped.Values; }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}