#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.Proxies
{
    public class LazySetProxy<T> : LazyCollectionProxyBase<T, ISet<T>>, ISet<T>
    {
        public LazySetProxy(string uri, IPomonaClient clientBase)
            : base(uri, clientBase)
        {
        }


        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return WrappedList.IsProperSubsetOf(other);
        }


        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return WrappedList.IsProperSupersetOf(other);
        }


        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return WrappedList.IsSubsetOf(other);
        }


        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return WrappedList.IsSupersetOf(other);
        }


        public bool Overlaps(IEnumerable<T> other)
        {
            return WrappedList.Overlaps(other);
        }


        public bool SetEquals(IEnumerable<T> other)
        {
            return WrappedList.SetEquals(other);
        }


        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }
    }
}