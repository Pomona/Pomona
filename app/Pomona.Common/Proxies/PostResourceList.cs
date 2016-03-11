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


        public int Count
        {
            get { return this.wrapped.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

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