#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

namespace Pomona.Common
{
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> dictionary;


        public ReadOnlyDictionary()
        {
            this.dictionary = new Dictionary<TKey, TValue>();
        }


        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        #region IDictionary<TKey,TValue> Members

        public TValue this[TKey key]
        {
            get { return this.dictionary[key]; }
            set { throw new NotSupportedException("This dictionary is read-only"); }
        }


        public ICollection<TKey> Keys
        {
            get { return this.dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return this.dictionary.Values; }
        }


        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }


        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }


        public bool Remove(TKey key)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public int Count
        {
            get { return this.dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }


        public void Clear()
        {
            throw new NotSupportedException("This dictionary is read-only");
        }


        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }


        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.dictionary as IEnumerable).GetEnumerator();
        }

        #endregion
    }
}