#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

namespace Pomona.Common.Linq.Queries.Interception
{
    internal class InterceptedQueryableSource<T> : InterceptedQueryable<T>, IInterceptedQueryableSource
    {
        internal InterceptedQueryableSource(InterceptedQueryProvider provider, IQueryable<T> wrappedSource)
            : base(provider, null)
        {
            if (wrappedSource == null)
                throw new ArgumentNullException(nameof(wrappedSource));
            WrappedSource = wrappedSource;
        }


        public IQueryable WrappedSource { get; }
    }
}