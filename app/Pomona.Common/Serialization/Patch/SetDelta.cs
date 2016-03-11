#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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