#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections;
using System.Collections.Generic;

namespace Pomona.Example
{
    public class CustomListWrapper<T> : IList<T>
    {
        private readonly List<T> wrappedList = new List<T>();

        #region IList<T> Members

        public T this[int index]
        {
            get { return this.wrappedList[index]; }
            set { this.wrappedList[index] = value; }
        }

        public int Count => this.wrappedList.Count;

        public bool IsReadOnly => ((IList<T>)this.wrappedList).IsReadOnly;


        public void Add(T item)
        {
            this.wrappedList.Add(item);
        }


        public void Clear()
        {
            this.wrappedList.Clear();
        }


        public bool Contains(T item)
        {
            return this.wrappedList.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            this.wrappedList.CopyTo(array, arrayIndex);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return this.wrappedList.GetEnumerator();
        }


        public int IndexOf(T item)
        {
            return this.wrappedList.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            this.wrappedList.Insert(index, item);
        }


        public bool Remove(T item)
        {
            return this.wrappedList.Remove(item);
        }


        public void RemoveAt(int index)
        {
            this.wrappedList.RemoveAt(index);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
