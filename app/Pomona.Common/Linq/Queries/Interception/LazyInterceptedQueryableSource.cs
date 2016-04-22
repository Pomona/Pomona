#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

namespace Pomona.Common.Linq.Queries.Interception
{
    public class LazyInterceptedQueryableSource<T> : InterceptedQueryable<T>, ILazyInterceptedQueryableSource
    {
        internal LazyInterceptedQueryableSource(InterceptedQueryProvider provider, Func<Type, IQueryable> factory)
            : base(provider, null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            Factory = factory;
        }


        public IQueryable ChangeType(Type elementType)
        {
            return ((InterceptedQueryProvider)Provider).CreateLazySource(elementType, Factory);
        }


        public Func<Type, IQueryable> Factory { get; }

        public IQueryable WrappedSource => Factory(typeof(T));
    }
}