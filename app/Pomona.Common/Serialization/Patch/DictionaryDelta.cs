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
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class DictionaryDelta<TKey, TValue, TDictionary> : Delta, IDictionary<TKey, TValue>, IDelta<TDictionary>,
                                                              IDictionaryDelta<TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly HashSet<TKey> removed = new HashSet<TKey>();
        private readonly IDictionary<TKey, TValue> replaced = new Dictionary<TKey, TValue>();

        public DictionaryDelta(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }

        public new TDictionary Original
        {
            get { return (TDictionary) base.Original; }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return
                Original.Where(x => !(removed.Contains(x.Key) || replaced.ContainsKey(x.Key)))
                        .Concat(replaced)
                        .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (ContainsKey(item.Key))
                throw new ArgumentException("Adding duplicate key not allowed.", "item");
            removed.Remove(item.Key);
            replaced.Add(item.Key, item.Value);
            SetDirty();
        }

        public void Clear()
        {
            if (Original.Count == 0)
                return;

            replaced.Clear();
            foreach (var key in Original.Keys)
            {
                removed.Add(key);
            }
            SetDirty();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (removed.Contains(item.Key))
                return false;

            if (replaced.ContainsKey(item.Key))
                return replaced.Contains(item);
            return Original.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.ToList().CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (removed.Contains(item.Key))
                return false;
            if ((replaced.ContainsKey(item.Key) && replaced.Remove(item)) || Original.Contains(item))
            {
                if (replaced.ContainsKey(item.Key))
                    return false;

                if (Original.ContainsKey(item.Key))
                {
                    removed.Add(item.Key);
                    SetDirty();
                }
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return Original.Keys.Concat(replaced.Keys).Except(removed).Distinct().Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(TKey key)
        {
            if (removed.Contains(key))
                return false;
            return replaced.ContainsKey(key) || Original.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            if (removed.Contains(key))
                return false;
            if (replaced.ContainsKey(key))
            {
                if (Original.ContainsKey(key))
                {
                    removed.Add(key);
                    SetDirty();
                }
                return true;
            }
            if (Original.ContainsKey(key))
            {
                removed.Add(key);
                SetDirty();
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            if (removed.Contains(key))
                return false;
            return replaced.TryGetValue(key, out value) || Original.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException("Key not found in delta dictionary.");
                return value;
            }
            set
            {
                removed.Remove(key);
                replaced[key] = value;
                SetDirty();
            }
        }

        public ICollection<TKey> Keys
        {
            get { return this.Select(x => x.Key).ToList(); }
        }

        public ICollection<TValue> Values
        {
            get { return this.Select(x => x.Value).ToList(); }
        }

        public IEnumerable<TKey> RemovedKeys
        {
            get { return removed; }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> ModifiedItems
        {
            get { return replaced; }
        }

        IDictionary<TKey, TValue> IDictionaryDelta<TKey, TValue>.Original
        {
            get { return Original; }
        }

        public override void Apply()
        {
            foreach (var removedKey in removed)
            {
                Original.Remove(removedKey);
            }
            foreach (var kvp in replaced)
            {
                Original[kvp.Key] = kvp.Value;
            }
            Reset();
        }

        public override void Reset()
        {
            removed.Clear();
            replaced.Clear();
            base.Reset();
        }
    }
}