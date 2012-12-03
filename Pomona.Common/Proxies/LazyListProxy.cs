#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

namespace Pomona.Common.Proxies
{
    public abstract class LazyListProxy
    {
        protected readonly ClientBase clientBase;
        protected readonly string uri;


        protected LazyListProxy(string uri, ClientBase clientBase)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            this.uri = uri;
            this.clientBase = clientBase;
        }


        internal static object CreateForType(Type elementType, string uri, ClientBase clientBase)
        {
            return Activator.CreateInstance(typeof (LazyListProxy<>).MakeGenericType(elementType), uri, clientBase);
        }
    }

    public class LazyListProxy<T> : LazyListProxy, IList<T>
    {
        private IList<T> dontTouchwrappedList;


        public LazyListProxy(string uri, ClientBase clientBase) : base(uri, clientBase)
        {
        }


        public IList<T> WrappedList
        {
            get
            {
                if (dontTouchwrappedList == null)
                    dontTouchwrappedList = clientBase.GetUri<IList<T>>(uri);
                return dontTouchwrappedList;
            }
        }

        #region IList<T> Members

        public T this[int index]
        {
            get { return WrappedList[index]; }
            set { throw new NotSupportedException("Not allowed to modify a REST'ed list"); }
        }


        public int Count
        {
            get { return WrappedList.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
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


        public IEnumerator<T> GetEnumerator()
        {
            return WrappedList.GetEnumerator();
        }


        public int IndexOf(T item)
        {
            return WrappedList.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public bool Remove(T item)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        public void RemoveAt(int index)
        {
            throw new NotSupportedException("Not allowed to modify a REST'ed list");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}