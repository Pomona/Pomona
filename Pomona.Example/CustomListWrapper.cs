using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomona.Example
{
    public class CustomListWrapper<T> : IList<T>
    {
        private List<T> wrappedList = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            return wrappedList.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(T item)
        {
            wrappedList.Add(item);
        }


        public void Clear()
        {
            wrappedList.Clear();
        }


        public bool Contains(T item)
        {
            return wrappedList.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
           wrappedList.CopyTo(array, arrayIndex);
        }


        public bool Remove(T item)
        {
            return wrappedList.Remove(item);
        }


        public int Count
        {
            get { return wrappedList.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<T>)wrappedList).IsReadOnly; }
        }

        public int IndexOf(T item)
        {
            return wrappedList.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            wrappedList.Insert(index, item);
        }


        public void RemoveAt(int index)
        {
            wrappedList.RemoveAt(index);
        }


        public T this[int index]
        {
            get { return wrappedList[index]; }
            set { wrappedList[index] = value; }
        }
    }
}
