#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

namespace Pomona.Common.Linq.Queries.Interception
{
    public interface ILazyInterceptedQueryableSource : IInterceptedQueryableSource
    {
        Func<Type, IQueryable> Factory { get; }
        IQueryable ChangeType(Type elementType);
    }
}