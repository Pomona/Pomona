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
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class DictionaryDelta<TKey, TValue, TDictionary>
        : Delta,
            IDictionary<TKey, TValue>,
            IDelta<TDictionary>,
            IDictionaryDelta<TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly HashSet<TKey> removed = new HashSet<TKey>();
        private readonly IDictionary<TKey, TValue> replaced = new Dictionary<TKey, TValue>();


        public DictionaryDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        public override void Reset()
        {
            this.removed.Clear();
            this.replaced.Clear();
            base.Reset();
        }


        public override void SetDirty()
        {
            if (this.replaced.Count == 0 && this.removed.Count == 0)
                ClearDirty();
            else
                base.SetDirty();
        }


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (ContainsKey(item.Key))
                throw new ArgumentException("Adding duplicate key not allowed.", nameof(item));
            this.removed.Remove(item.Key);
            this.replaced.Add(item.Key, item.Value);
            SetDirty();
        }


        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }


        public override void Apply()
        {
            foreach (var removedKey in this.removed)
                Original.Remove(removedKey);
            foreach (var kvp in this.replaced)
                Original[kvp.Key] = kvp.Value;
            Reset();
        }


        public void Clear()
        {
            if (Original.Count == 0)
                return;

            this.replaced.Clear();
            foreach (var key in Original.Keys)
                this.removed.Add(key);
            SetDirty();
        }


        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this.removed.Contains(item.Key))
                return false;

            if (this.replaced.ContainsKey(item.Key))
                return this.replaced.Contains(item);
            return Original.Contains(item);
        }


        public bool ContainsKey(TKey key)
        {
            if (this.removed.Contains(key))
                return false;
            return this.replaced.ContainsKey(key) || Original.ContainsKey(key);
        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.ToList().CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return Original.Keys.Concat(this.replaced.Keys).Except(this.removed).Distinct().Count(); }
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return
                Original.Where(x => !(this.removed.Contains(x.Key) || this.replaced.ContainsKey(x.Key)))
                        .Concat(this.replaced)
                        .GetEnumerator();
        }


        public bool IsReadOnly
        {
            get { return false; }
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
                this.removed.Remove(key);
                TValue originalValue;
                if (Original.TryGetValue(key, out originalValue) && Equals(originalValue, value))
                    this.replaced.Remove(key);
                else
                    this.replaced[key] = value;
                SetDirty();
            }
        }

        public ICollection<TKey> Keys
        {
            get { return this.Select(x => x.Key).ToList(); }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> ModifiedItems
        {
            get { return this.replaced; }
        }

        public new TDictionary Original
        {
            get { return (TDictionary)base.Original; }
        }


        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this.removed.Contains(item.Key))
                return false;
            if ((this.replaced.ContainsKey(item.Key) && this.replaced.Remove(item)) || Original.Contains(item))
            {
                if (this.replaced.ContainsKey(item.Key))
                    return false;

                if (Original.ContainsKey(item.Key))
                {
                    this.removed.Add(item.Key);
                    SetDirty();
                }
                return true;
            }
            return false;
        }


        public bool Remove(TKey key)
        {
            if (this.removed.Contains(key))
                return false;
            if (this.replaced.ContainsKey(key))
            {
                if (Original.ContainsKey(key))
                {
                    this.removed.Add(key);
                    SetDirty();
                }
                return true;
            }
            if (Original.ContainsKey(key))
            {
                this.removed.Add(key);
                SetDirty();
                return true;
            }
            return false;
        }


        public IEnumerable<TKey> RemovedKeys
        {
            get { return this.removed; }
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            if (this.removed.Contains(key))
                return false;
            return this.replaced.TryGetValue(key, out value) || Original.TryGetValue(key, out value);
        }


        public ICollection<TValue> Values
        {
            get { return this.Select(x => x.Value).ToList(); }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        IDictionary<TKey, TValue> IDictionaryDelta<TKey, TValue>.Original
        {
            get { return Original; }
        }
    }
}