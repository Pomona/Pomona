using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

namespace Pomona.Common.Internals
{
    public class CastingListWrapper<TOuter> : IList<TOuter>
        where TOuter : class
    {
        private IList inner;

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


        public int Count
        {
            get { return inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return inner.IsReadOnly; }
        }

        #endregion

        #region Implementation of IList<TOuter>

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


        public TOuter this[int index]
        {
            get { return (TOuter)inner[index]; }
            set { inner[index] = value; }
        }

        #endregion
    }
}