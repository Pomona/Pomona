#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json.Linq;

namespace Pomona.Common.Linq
{
    public static class RestQueryExtensions
    {
        public static IQueryable<TSource> Expand<TSource, TProperty>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource), typeof(TProperty));
            var property = Expression.Quote(propertySelector);
            var methodCallExpression = Expression.Call(null, genericMethod, new[]
            {
                source.Expression,
                property
            });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> Expand<TSource, TProperty>(this IEnumerable<TSource> source,
            Func<TSource, TProperty> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source;
        }


        public static TSource FirstLazy<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });
            
            return source.Provider.Execute<TSource>(methodCallExpression);
        }


        public static TSource FirstLazy<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.First();
        }


        public static IQueryable<TSource> IncludeTotalCount<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)),
                    new[] { source.Expression }
                    ));
        }


        public static IEnumerable<TSource> IncludeTotalCount<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source;
        }


        public static IQueryable<TSource> WithOptions<TSource>(this IQueryable<TSource> source, Action<IRequestOptions> optionsModifier)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)),
                    new[] { source.Expression, Expression.Constant(optionsModifier) }
                    ));
        }


        public static IEnumerable<TSource> WithOptions<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source;
        }


        public static JToken ToJson<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var methodCallExpression =
                Expression.Call(null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] { typeof(TSource) }),
                    new[] { source.Expression });
            return source.Provider.Execute<JObject>(methodCallExpression);
        }


        public static QueryResult<TSource> ToQueryResult<TSource>(this IQueryable<TSource> source)
        {

            if (source == null)
                throw new ArgumentNullException("source");
            var methodCallExpression =
                Expression.Call(null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] { typeof(TSource) }),
                    new[] { source.Expression });
            return source.Provider.Execute<QueryResult<TSource>>(methodCallExpression);
        }

        public static QueryResult<TSource> ToQueryResult<TSource>(this IEnumerable<TSource> source)
        {
            return new QueryResult<TSource>(source,0,source.Count(),null);
        }

        public static Uri ToUri<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var methodCallExpression =
                Expression.Call(null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] { typeof(TSource) }),
                    new[] { source.Expression });
            return source.Provider.Execute<Uri>(methodCallExpression);
        }
    }
}