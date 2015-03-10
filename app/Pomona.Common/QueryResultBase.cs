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
using System.Collections.ObjectModel;
using System.Linq;

namespace Pomona.Common
{
    public abstract class QueryResultBase<T, TCollection> : QueryResult, ICollection<T>
        where TCollection : ICollection<T>
    {
        protected readonly TCollection items;
        private readonly string next;
        private readonly string previous;
        private readonly int skip;
        private readonly int totalCount;


        protected QueryResultBase(TCollection items, int skip, int totalCount, string previous, string next)
        {
            this.items = items;
            this.skip = skip;
            this.totalCount = totalCount;
            this.previous = previous;
            this.next = next;
        }


        public override Type ItemType
        {
            get { return typeof(T); }
        }

        public override Type ListType
        {
            get { return typeof(TCollection); }
        }

        public override string Next
        {
            get { return this.next; }
        }

        public override string Previous
        {
            get { return this.previous; }
        }

        public override int Skip
        {
            get { return this.skip; }
        }

        public override int TotalCount
        {
            get { return this.totalCount; }
        }

        #region IList<T> Members

        public override int Count
        {
            get { return this.items.Count; }
        }

        // For serialization
        internal IEnumerable<T> Items
        {
            get { return new ReadOnlyCollection<T>(this.ToList()); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }


        public void Add(T item)
        {
            throw new NotSupportedException();
        }


        public void Clear()
        {
            throw new NotSupportedException();
        }


        public bool Contains(T item)
        {
            return this.items.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }


        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        #endregion
    }
}