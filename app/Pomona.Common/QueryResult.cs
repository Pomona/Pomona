#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Serialization;

namespace Pomona.Common
{
    public abstract class QueryResult : IPomonaSerializable
    {
        private static readonly MethodInfo createMethod;


        static QueryResult()
        {
            createMethod =
                ReflectionHelper.GetMethodDefinition<QueryResult>(x => Create<object>(null, 0, 0, null, null));
        }


        public abstract int Count { get; }

        public Dictionary<string, string> DebugInfo { get; } = new Dictionary<string, string>();

        public abstract Type ItemType { get; }
        public abstract Type ListType { get; }
        public abstract string Next { get; }
        public abstract string Previous { get; }
        public abstract int Skip { get; }
        public abstract int TotalCount { get; }


        public static QueryResult Create(
            IEnumerable source,
            int skip,
            int totalCount,
            string previousPageUrl,
            string nextPageUrl,
            Type elementType = null)
        {
            Type[] genargs;
            if (elementType == null)
            {
                if (!TypeUtils.TryGetTypeArguments(source.GetType(), typeof(IEnumerable<>), out genargs))
                {
                    var asQueryable = source as IQueryable;
                    if (asQueryable == null)
                        throw new ArgumentException("source needs to implement IQueryable or IEnumerable<>");
                    elementType = asQueryable.ElementType;
                }
                else
                    elementType = genargs[0];
            }

            return
                (QueryResult)
                    createMethod.MakeGenericMethod(elementType).Invoke(
                        null,
                        new object[] { source, skip, totalCount, previousPageUrl, nextPageUrl });
        }


        private static QueryResult Create<TSource>(IEnumerable source, int skip, int totalCount, string previousPageUrl, string nextPageUrl)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var castSource = source as IEnumerable<TSource> ?? source.Cast<TSource>();
            Type[] tmp;
            var isSetCollection = source.GetType().TryExtractTypeArguments(typeof(ISet<>), out tmp);
            return isSetCollection
                ? (QueryResult)new QuerySetResult<TSource>(castSource, skip, totalCount, previousPageUrl, nextPageUrl)
                : new QueryResult<TSource>(castSource, skip, totalCount, previousPageUrl, nextPageUrl);
        }


        public bool PropertyIsSerialized(string propertyName)
        {
            if (propertyName == nameof(DebugInfo) && (DebugInfo == null || DebugInfo.Count == 0))
                return false;
            return true;
        }
    }

    public class QueryResult<T> : QueryResultBase<T, IList<T>>, IList<T>
    {
        public QueryResult(IEnumerable<T> items, int skip, int totalCount, string previous, string next)
            : base(items.ToList(), skip, totalCount, previous, next)
        {
        }


        public int IndexOf(T item)
        {
            return this.items.IndexOf(item);
        }


        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }


        public T this[int index]
        {
            get { return this.items[index]; }
            set { throw new NotSupportedException(); }
        }


        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
    }
}