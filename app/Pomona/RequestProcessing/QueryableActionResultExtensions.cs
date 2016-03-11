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