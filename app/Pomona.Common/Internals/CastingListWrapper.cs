#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

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
            return this.inner.Cast<TOuter>().GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<TOuter>

        public int Count
        {
            get { return this.inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.inner.IsReadOnly; }
        }


        public void Add(TOuter item)
        {
            this.inner.Add(item);
        }


        public void Clear()
        {
            this.inner.Clear();
        }


        public bool Contains(TOuter item)
        {
            return this.inner.Contains(item);
        }


        public void CopyTo(TOuter[] array, int arrayIndex)
        {
            this.inner.Cast<TOuter>().ToList().CopyTo(array, arrayIndex);
        }


        public bool Remove(TOuter item)
        {
            this.inner.Remove(item);
            return true;
        }

        #endregion

        #region Implementation of IList<TOuter>

        public TOuter this[int index]
        {
            get { return (TOuter)this.inner[index]; }
            set { this.inner[index] = value; }
        }


        public int IndexOf(TOuter item)
        {
            return this.inner.IndexOf(item);
        }


        public void Insert(int index, TOuter item)
        {
            this.inner.Insert(index, item);
        }


        public void RemoveAt(int index)
        {
            this.inner.RemoveAt(index);
        }

        #endregion
    }
}