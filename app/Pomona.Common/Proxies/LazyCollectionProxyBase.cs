#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

using Pomona.Common.Loading;

namespace Pomona.Common.Proxies
{
    public abstract class LazyCollectionProxyBase<T, TCollection> : LazyCollectionProxy, ICollection<T>
        where TCollection : ICollection<T>
    {
        private TCollection dontTouchwrappedList;


        public LazyCollectionProxyBase(string uri, IResourceLoader resourceLoader)
            : base(uri, resourceLoader)
        {
        }


        public override bool IsLoaded
        {
            get { return this.dontTouchwrappedList != null; }
        }

        public TCollection WrappedList
        {
            get
            {
                if (this.dontTouchwrappedList == null)
                    this.dontTouchwrappedList = ResourceLoader.Get<TCollection>(Uri);

                return this.dontTouchwrappedList;
            }
        }


        public void Add(T item)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public void Clear()
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public bool Contains(T item)
        {
            return WrappedList.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            WrappedList.CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return WrappedList.Count; }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return WrappedList.GetEnumerator();
        }


        public bool IsReadOnly
        {
            get { return true; }
        }


        public bool Remove(T item)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}