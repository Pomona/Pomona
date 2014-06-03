#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

namespace Pomona.Common.Internals
{
    public class MappedReadOnlyCollection<TOuter, TInner> : ICollection<TOuter>
    {
        private readonly ICollection<TInner> inner;

        private readonly Func<TInner, TOuter> map;


        public MappedReadOnlyCollection(ICollection<TInner> inner, Func<TInner, TOuter> map)
        {
            if (inner == null)
                throw new ArgumentNullException("inner");
            if (map == null)
                throw new ArgumentNullException("map");
            this.inner = inner;
            this.map = map;
        }


        public int Count
        {
            get { return this.inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        protected ICollection<TInner> Inner
        {
            get { return this.inner; }
        }

        protected Func<TInner, TOuter> Map
        {
            get { return this.map; }
        }


        public void Add(TOuter item)
        {
            throw new NotSupportedException("Collection is read-only");
        }


        public void Clear()
        {
            throw new NotSupportedException("Collection is read-only");
        }


        public bool Contains(TOuter item)
        {
            return this.inner.Select(this.map).Contains(item);
        }


        public void CopyTo(TOuter[] array, int arrayIndex)
        {
            this.inner.Select(this.map).ToList().CopyTo(array, arrayIndex);
        }


        public IEnumerator<TOuter> GetEnumerator()
        {
            return this.inner.Select(this.map).GetEnumerator();
        }


        public bool Remove(TOuter item)
        {
            throw new NotSupportedException("Collection is read-only");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}