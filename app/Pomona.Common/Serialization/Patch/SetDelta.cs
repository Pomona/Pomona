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
    public class SetDelta<TElement, TSet> : SetDelta<TElement>, IDelta<TSet>
    {
        public SetDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        public new TSet Original => (TSet)base.Original;
    }

    public class SetDelta<TElement> : CollectionDelta<TElement>, ISet<TElement>
    {
        public SetDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected SetDelta()
        {
        }


        public new bool Add(TElement item)
        {
            return AddItem(item);
        }


        public void ExceptWith(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            foreach (var item in other)
            {
                if (Contains(item))
                    Remove(item);
            }
        }


        public void IntersectWith(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var otherSet = new HashSet<TElement>(other);

            foreach (var item in this.ToList())
            {
                if (!otherSet.Contains(item))
                    Remove(item);
            }
        }


        public bool IsProperSubsetOf(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return new HashSet<TElement>(this).IsProperSubsetOf(other);
        }


        public bool IsProperSupersetOf(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return new HashSet<TElement>(this).IsProperSupersetOf(other);
        }


        public bool IsSubsetOf(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return new HashSet<TElement>(this).IsSubsetOf(other);
        }


        public bool IsSupersetOf(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return new HashSet<TElement>(this).IsSupersetOf(other);
        }


        public bool Overlaps(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return new HashSet<TElement>(this).Overlaps(other);
        }


        public bool SetEquals(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return new HashSet<TElement>(this).SetEquals(other);
        }


        public void SymmetricExceptWith(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            foreach (var item in other.Distinct())
            {
                if (Contains(item))
                    Remove(item);
                else
                    Add(item);
            }
        }


        public void UnionWith(IEnumerable<TElement> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            foreach (var item in other)
                Add(item);
        }
    }
}