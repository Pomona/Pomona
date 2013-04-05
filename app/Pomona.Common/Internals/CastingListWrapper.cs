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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.Internals
{
    public class CastingListWrapper<TOuter> : IList<TOuter>
        where TOuter : class
    {
        private readonly IList inner;

        #region Implementation of IEnumerable

        public CastingListWrapper(IList inner)
        {
            this.inner = inner;
        }


        public IEnumerator<TOuter> GetEnumerator()
        {
            return inner.Cast<TOuter>().GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<TOuter>

        public int Count
        {
            get { return inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return inner.IsReadOnly; }
        }


        public void Add(TOuter item)
        {
            inner.Add(item);
        }


        public void Clear()
        {
            inner.Clear();
        }


        public bool Contains(TOuter item)
        {
            return inner.Contains(item);
        }


        public void CopyTo(TOuter[] array, int arrayIndex)
        {
            inner.Cast<TOuter>().ToList().CopyTo(array, arrayIndex);
        }


        public bool Remove(TOuter item)
        {
            inner.Remove(item);
            return true;
        }

        #endregion

        #region Implementation of IList<TOuter>

        public TOuter this[int index]
        {
            get { return (TOuter) inner[index]; }
            set { inner[index] = value; }
        }


        public int IndexOf(TOuter item)
        {
            return inner.IndexOf(item);
        }


        public void Insert(int index, TOuter item)
        {
            inner.Insert(index, item);
        }


        public void RemoveAt(int index)
        {
            inner.RemoveAt(index);
        }

        #endregion
    }
}