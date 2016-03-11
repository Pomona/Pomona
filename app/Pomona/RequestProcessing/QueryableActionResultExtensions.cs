#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Linq.NonGeneric;

namespace Pomona.RequestProcessing
{
    internal static class QueryableActionResultExtensions
    {
        private static readonly MethodInfo wrapActionResultGenericMethod =
            ReflectionHelper.GetMethodDefinition(() => WrapActionResult<object, object>(null, null, null));


        public static IQueryableActionResult<T> WrapActionResult<T>(this IQueryable<T> source,
                                                                    QueryProjection projection = null,
                                                                    int? defaultPageSize = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return (IQueryableActionResult<T>)WrapActionResult((IQueryable)source, projection, defaultPageSize);
        }


        public static IQueryableActionResult WrapActionResult(this IQueryable source,
                                                              QueryProjection projection = null,
                                                              int? defaultPageSize = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Type resultType;
            if (projection != null)
                resultType = projection.GetResultType(source.ElementType);
            else
            {
                projection = QueryProjection.AsEnumerable;
                resultType = source.ElementType;
            }
            return
                (IQueryableActionResult)wrapActionResultGenericMethod.MakeGenericMethod(source.ElementType, resultType)
                                                                     .Invoke(null, new object[] { source, projection, defaultPageSize });
        }


        private static IQueryableActionResult WrapActionResult<TElement, TResult>(IQueryable<TElement> source,
                                                                                  QueryProjection projection,
                                                                                  int? defaultPageSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new QueryableActionResult<TElement, TResult>(source, projection, defaultPageSize);
        }
    }
}