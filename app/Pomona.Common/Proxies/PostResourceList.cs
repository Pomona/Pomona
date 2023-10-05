#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Pomona.Common.Proxies
{
    internal class PostResourceList<T> : IList<T>
    {
        private readonly PostResourceBase owner;
        private readonly string propertyName;
        private readonly List<T> wrapped = new List<T>();


        internal PostResourceList(PostResourceBase owner, string propertyName)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            this.owner = owner;
            this.propertyName = propertyName;
        }


        private void SetDirty()
        {
            this.owner.SetDirty(this.propertyName);
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return this.wrapped.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>

        public void Add(T item)
        {
            this.wrapped.Add(item);
            SetDirty();
        }


        public void Clear()
        {
            if (this.wrapped.Count > 0)
            {
                this.wrapped.Clear();
                SetDirty();
            }
        }


        public bool Contains(T item)
        {
            return this.wrapped.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            this.wrapped.CopyTo(array, arrayIndex);
        }


        public bool Remove(T item)
        {
            return this.wrapped.Remove(item);
        }


        public int Count => this.wrapped.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            return this.wrapped.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            this.wrapped.Insert(index, item);
            SetDirty();
        }


        public void RemoveAt(int index)
        {
            this.wrapped.RemoveAt(index);
            SetDirty();
        }


        public T this[int index]
        {
            get { return this.wrapped[index]; }
            set
            {
                this.wrapped[index] = value;
                SetDirty();
            }
        }

        #endregion
    }
}
