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
using System.Collections.Specialized;
using System.Globalization;

namespace Pomona.Common
{
    public abstract class QueryResultBase<T, TCollection> : QueryResult, ICollection<T>
        where TCollection : ICollection<T>
    {
        protected readonly TCollection items;
        private readonly int skip;
        private readonly int totalCount;
        private readonly string url;


        protected QueryResultBase(TCollection items, int skip, int totalCount, string url)
        {
            this.items = items;
            this.skip = skip;
            this.totalCount = totalCount;
            this.url = url;
        }


        public override Type ItemType
        {
            get { return typeof(T); }
        }

        public override Type ListType
        {
            get { return typeof(TCollection); }
        }

        public override int Skip
        {
            get { return this.skip; }
        }

        public override int TotalCount
        {
            get { return this.totalCount; }
        }

        public override string Url
        {
            get { return this.url; }
        }


        public override bool TryGetPage(int offset, out Uri pageUri)
        {
            var newSkip = Math.Max(Skip + (Count * offset), 0);
            var uriBuilder = new UriBuilder(Url);

            if (Skip == newSkip || (TotalCount != -1 && newSkip >= TotalCount))
            {
                pageUri = null;
                return false;
            }

            NameValueCollection parameters;
            if (!string.IsNullOrEmpty(uriBuilder.Query))
            {
                parameters = HttpUtility.ParseQueryString(uriBuilder.Query);
                parameters["$skip"] = newSkip.ToString(CultureInfo.InvariantCulture);
                uriBuilder.Query = parameters.ToString();
            }
            else
                uriBuilder.Query = "$skip=" + newSkip;

            pageUri = uriBuilder.Uri;

            return true;
        }

        #region IList<T> Members

        public override int Count
        {
            get { return this.items.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }


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