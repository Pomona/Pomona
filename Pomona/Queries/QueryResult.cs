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
using System.Linq;

namespace Pomona.Queries
{
    public class QueryResult<T> : IList<T>
    {
        private readonly List<T> items;
        private readonly int skip;

        private readonly int totalCount;


        public QueryResult(IEnumerable<T> items, int skip, int totalCount)
        {
            this.items = items.ToList();
            this.skip = skip;
            this.totalCount = totalCount;
        }


        public int Skip
        {
            get { return this.skip; }
        }

        public int TotalCount
        {
            get { return this.totalCount; }
        }

        #region IList<T> Members

        public T this[int index]
        {
            get { return this.items[index]; }
            set { throw new NotSupportedException(); }
        }


        public int Count
        {
            get { return this.items.Count; }
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


        public int IndexOf(T item)
        {
            return this.items.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }


        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }


        public void RemoveAt(int index)
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