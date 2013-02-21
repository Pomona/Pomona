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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Linq
{
    public static class RestQueryExtensions
    {
        public static QueryResult<TSource> ToQueryResult<TSource>(this IQueryable<TSource> source)
        {
            return (QueryResult<TSource>) source.Provider.Execute(source.Expression);
        }

        public static IQueryable<TSource> IncludeTotalCount<TSource>(this IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TSource)),
                    new[] {source.Expression}
                    ));
        }

        public static IQueryable<TSource> Expand<TSource, TProperty>(this IQueryable<TSource> source,
                                                                     Expression<Func<TSource, TProperty>>
                                                                         propertySelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertySelector == null) throw new ArgumentNullException("propertySelector");
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TSource), typeof (TProperty)),
                    new[] {source.Expression, Expression.Quote(propertySelector)}
                    ));
        }

        public static IEnumerable<TSource> Expand<TSource, TProperty>(this IEnumerable<TSource> source,
                                                                      Func<TSource, TProperty> propertySelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source;
        }
    }
}