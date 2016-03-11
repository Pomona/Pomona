#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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
        private readonly ExtendedResourceInfo userTypeInfo;
        private readonly IList<TServer> wrappedList;
        private readonly Dictionary<TServer, TExtended> wrapperMap = new Dictionary<TServer, TExtended>();

        public ExtendedResourceInfo UserTypeInfo
        {
            get { return this.userTypeInfo; }
        }

        public object WrappedResource
        {
            get { return this.wrappedList; }
        }

        #region IList<T> Members

        public ExtendedResourceList(IList<TServer> wrappedList, ExtendedResourceInfo userTypeInfo)
        {
            if (wrappedList == null)
                throw new ArgumentNullException(nameof(wrappedList));
            if (userTypeInfo == null)
                throw new ArgumentNullException(nameof(userTypeInfo));
            this.wrappedList = wrappedList;
            this.userTypeInfo = userTypeInfo;
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

        public int Count
        {
            get { return this.wrappedList.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<TExtended>)this.wrappedList).IsReadOnly; }
        }


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