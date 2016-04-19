#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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


        protected QueryResultBase(TCollection items, int skip, int totalCount, string previous, string next)
        {
            this.items = items;
            Skip = skip;
            TotalCount = totalCount;
            Previous = previous;
            Next = next;
        }


        public override Type ItemType => typeof(T);

        public override Type ListType => typeof(TCollection);

        public override string Next { get; }

        public override string Previous { get; }

        public override int Skip { get; }

        public override int TotalCount { get; }

        #region IList<T> Members

        public override int Count => this.items.Count;

        // For serialization
        internal IEnumerable<T> Items => new ReadOnlyCollection<T>(this.ToList());

        public bool IsReadOnly => true;


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