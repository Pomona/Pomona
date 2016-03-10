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
using System.Collections.Generic;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class ListDelta<TElement, TList> : ListDelta<TElement>, IDelta<TList>
    {
        public ListDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        public new TList Original => (TList)base.Original;
    }

    public class ListDelta<TElement> : CollectionDelta<TElement>, IList<TElement>
    {
        public ListDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected ListDelta()
        {
        }


        public int IndexOf(TElement item)
        {
            return
                this.Select((x, i) => new { x, i }).Where(y => y.x.Equals(item)).Select(y => (int?)y.i).FirstOrDefault()
                ?? -1;
        }


        public void Insert(int index, TElement item)
        {
            AddItem(item);
        }


        public TElement this[int index]
        {
            get { return this.Skip(index).First(); }
            set { throw new NotImplementedException(); }
        }


        public void RemoveAt(int index)
        {
            var item = this.Skip(index).FirstOrDefault();
            if (item != null)
                RemoveItem(item);
        }
    }
}