#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries.Interception
{
    public static class InterceptedQueryableExtensions
    {
        public static IQueryable<T> InterceptWith<T>(this IQueryable<T> source, params ExpressionVisitor[] visitors)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new InterceptedQueryableSource<T>(new InterceptedQueryProvider(visitors), source);
        }
    }
}