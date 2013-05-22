// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Pomona.Common.Internals;
using Pomona.Internals;

namespace Pomona.Common
{
    public abstract class QueryResult
    {
        private static readonly MethodInfo createMethod;

        private readonly Dictionary<string, string> debugInfo = new Dictionary<string, string>();

        static QueryResult()
        {
            createMethod =
                ReflectionHelper.GetGenericMethodDefinition<QueryResult>(x => Create<object>(null, 0, 0, null));
        }

        public Dictionary<string, string> DebugInfo
        {
            get { return debugInfo; }
        }


        public abstract int Count { get; }
        public abstract Type ListType { get; }
        public abstract int Skip { get; }
        public abstract int TotalCount { get; }
        public abstract bool TryGetPage(int offset, out Uri pageUri);


        public static QueryResult Create(IEnumerable source, int skip, int totalCount, string url)
        {
            Type elementType;
            Type[] genargs;
            if (!TypeUtils.TryGetTypeArguments(source.GetType(), typeof (IEnumerable<>), out genargs))
            {
                var asQueryable = source as IQueryable;
                if (asQueryable == null)
                {
                    throw new ArgumentException("source needs to implement IQuerable or IEnumerable<>");
                }
                elementType = asQueryable.ElementType;
            }
            else
            {
                elementType = genargs[0];
            }
            return
                (QueryResult)
                createMethod.MakeGenericMethod(elementType).Invoke(
                    null, new object[] {source, skip, totalCount, url});
        }


        private static QueryResult Create<TSource>(IEnumerable<TSource> source, int skip, int totalCount, string url)
        {
            return new QueryResult<TSource>(source, skip, totalCount, url);
        }
    }

    public class QueryResult<T> : QueryResult, IList<T>
    {
        private readonly List<T> items;
        private readonly int skip;

        private readonly int totalCount;
        private readonly string url;


        public QueryResult(IEnumerable<T> items, int skip, int totalCount, string url)
        {
            this.items = items.ToList();
            this.skip = skip;
            this.totalCount = totalCount;
            this.url = url;
        }


        public override Type ListType
        {
            get { return typeof (IList<T>); }
        }

        public override int Skip
        {
            get { return skip; }
        }

        public override int TotalCount
        {
            get { return totalCount; }
        }

        public string Url
        {
            get { return url; }
        }

        #region IList<T> Members

        public T this[int index]
        {
            get { return items[index]; }
            set { throw new NotSupportedException(); }
        }


        public override int Count
        {
            get { return items.Count; }
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
            return items.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }


        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }


        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }


        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        public override bool TryGetPage(int offset, out Uri pageUri)
        {
            var newSkip = Math.Max(Skip + (Count*offset), 0);
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
    }
}