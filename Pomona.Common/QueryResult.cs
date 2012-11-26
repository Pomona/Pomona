using System;

namespace Pomona.Common
{
    public abstract class QueryResult
    {
        public abstract int Count { get; }
        public abstract Type ListType { get; }
        public abstract int Skip { get; }
        public abstract int TotalCount { get; }
        public abstract bool TryGetPage(int offset, out Uri pageUri);
    }
}