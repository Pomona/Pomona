#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.Proxies
{
    public class LazyListProxy<T> : LazyCollectionProxyBase<T, IList<T>>, IList<T>
    {
        public LazyListProxy(string uri, IPomonaClient clientBase)
            : base(uri, clientBase)
        {
        }

        #region IList<T> Members

        public T this[int index]
        {
            get { return WrappedList[index]; }
            set { throw new NotSupportedException("Not allowed to modify a REST'ed list"); }
        }


        public int IndexOf(T item)
        {
            return WrappedList.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public void RemoveAt(int index)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }

        #endregion
    }
}