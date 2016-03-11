#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common
{
    public class QuerySetResult<T> : QueryResultBase<T, ISet<T>>, ISet<T>
    {
        public QuerySetResult(IEnumerable<T> items, int skip, int totalCount, string previous, string next)
            : base(new HashSet<T>(items), skip, totalCount, previous, next)
        {
        }


        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }


        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }


        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.items.IsProperSubsetOf(other);
        }


        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return this.items.IsProperSupersetOf(other);
        }


        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.items.IsSubsetOf(other);
        }


        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return this.items.IsSupersetOf(other);
        }


        public bool Overlaps(IEnumerable<T> other)
        {
            return this.items.Overlaps(other);
        }


        public bool SetEquals(IEnumerable<T> other)
        {
            return this.items.SetEquals(other);
        }


        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }


        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }


        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }
    }
}