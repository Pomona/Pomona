#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2016 Karsten Nikolai Strand
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
    internal class PostResourceSet<T> : ISet<T>
    {
        private readonly PostResourceBase owner;
        private readonly string propertyName;
        private readonly ISet<T> wrapped = new HashSet<T>();


        internal PostResourceSet(PostResourceBase owner, string propertyName)
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


        public bool Add(T item)
        {
            var added = this.wrapped.Add(item);
            if (added)
                SetDirty();
            return added;
        }


        public void Clear()
        {
            if (this.wrapped.Count > 0)
                SetDirty();
            this.wrapped.Clear();
        }


        public bool Contains(T item)
        {
            return this.wrapped.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            this.wrapped.CopyTo(array, arrayIndex);
        }


        public int Count => this.wrapped.Count;


        public void ExceptWith(IEnumerable<T> other)
        {
            this.wrapped.ExceptWith(other);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return this.wrapped.GetEnumerator();
        }


        public void IntersectWith(IEnumerable<T> other)
        {
            this.wrapped.IntersectWith(other);
        }


        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.wrapped.IsProperSubsetOf(other);
        }


        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return this.wrapped.IsProperSupersetOf(other);
        }


        public bool IsReadOnly => this.wrapped.IsReadOnly;


        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.wrapped.IsSubsetOf(other);
        }


        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return this.wrapped.IsSupersetOf(other);
        }


        public bool Overlaps(IEnumerable<T> other)
        {
            return this.wrapped.Overlaps(other);
        }


        public bool Remove(T item)
        {
            var removed = this.wrapped.Remove(item);
            if (removed)
                SetDirty();
            return removed;
        }


        public bool SetEquals(IEnumerable<T> other)
        {
            return this.wrapped.SetEquals(other);
        }


        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            this.wrapped.SymmetricExceptWith(other);
        }


        public void UnionWith(IEnumerable<T> other)
        {
            this.wrapped.UnionWith(other);
        }


        void ICollection<T>.Add(T item)
        {
            Add(item);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.wrapped).GetEnumerator();
        }
    }
}