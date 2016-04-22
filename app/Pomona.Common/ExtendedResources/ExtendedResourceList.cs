#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal class ExtendedResourceList<TExtended, TServer> : IList<TExtended>, IExtendedResourceProxy
        where TExtended : TServer, IClientResource
        where TServer : IClientResource
    {
        private readonly IList<TServer> wrappedList;
        private readonly Dictionary<TServer, TExtended> wrapperMap = new Dictionary<TServer, TExtended>();

        public ExtendedResourceInfo UserTypeInfo { get; }

        public object WrappedResource => this.wrappedList;

        #region IList<T> Members

        public ExtendedResourceList(IList<TServer> wrappedList, ExtendedResourceInfo userTypeInfo)
        {
            if (wrappedList == null)
                throw new ArgumentNullException(nameof(wrappedList));
            if (userTypeInfo == null)
                throw new ArgumentNullException(nameof(userTypeInfo));
            this.wrappedList = wrappedList;
            UserTypeInfo = userTypeInfo;
        }


        private TServer Unwrap(TExtended outerItem, bool cacheWrapper = false)
        {
            var proxy = (IExtendedResourceProxy)outerItem;
            if (proxy == null)
                throw new InvalidOperationException("Unable to unwrap item, does not implemented IExtendedResourceProxy");
            var innerItem = (TServer)proxy.WrappedResource;
            if (cacheWrapper)
                this.wrapperMap[innerItem] = outerItem;
            return innerItem;
        }


        private TExtended Wrap(TServer innerItem)
        {
            TExtended outerItem;
            if (this.wrapperMap.TryGetValue(innerItem, out outerItem))
                return outerItem;
            outerItem = innerItem.Wrap<TServer, TExtended>();
            this.wrapperMap[innerItem] = outerItem;
            return outerItem;
        }


        public TExtended this[int index]
        {
            get { return Wrap(this.wrappedList[index]); }
            set { throw new NotImplementedException(); }
        }

        public int Count => this.wrappedList.Count;

        public bool IsReadOnly => ((IList<TExtended>)this.wrappedList).IsReadOnly;


        public void Add(TExtended item)
        {
            this.wrappedList.Add(Unwrap(item, true));
        }


        public void Clear()
        {
            this.wrapperMap.Clear();
            this.wrappedList.Clear();
        }


        public bool Contains(TExtended item)
        {
            return this.wrappedList.Contains(Unwrap(item));
        }


        public void CopyTo(TExtended[] array, int arrayIndex)
        {
            this.wrappedList.Select(Wrap).ToArray().CopyTo(array, arrayIndex);
        }


        public IEnumerator<TExtended> GetEnumerator()
        {
            return this.wrappedList.Select(Wrap).GetEnumerator();
        }


        public int IndexOf(TExtended item)
        {
            return this.wrappedList.IndexOf(Unwrap(item));
        }


        public void Insert(int index, TExtended item)
        {
            this.wrappedList.Insert(index, Unwrap(item, true));
        }


        public bool Remove(TExtended item)
        {
            return this.wrappedList.Remove(Unwrap(item));
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