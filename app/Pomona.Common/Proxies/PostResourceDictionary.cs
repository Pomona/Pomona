using System;
using System.Collections;
using System.Collections.Generic;

namespace Pomona.Common.Proxies
{
    internal class PostResourceDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly string propertyName;
        private PutResourceBase owner;
        private IDictionary<TKey, TValue> wrapped = new Dictionary<TKey, TValue>();

        internal PostResourceDictionary(PutResourceBase owner, string propertyName)
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