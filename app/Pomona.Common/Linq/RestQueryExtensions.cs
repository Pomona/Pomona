#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Linq
{
    public static class RestQueryExtensions
    {
        public static IQueryable<TSource> Expand<TSource, TProperty>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource), typeof(TProperty));
            var property = Expression.Quote(propertySelector);
            var methodCallExpression = Expression.Call(null,
                                                       genericMethod,
                                                       new[]
                                                       {
                                                           source.Expression,
                                                           property
                                                       });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> Expand<TSource, TProperty>(this IEnumerable<TSource> source,
                                                                      Func<TSource, TProperty> propertySelector,
                                                                      ExpandMode expandMode)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            switch (expandMode)
            {
                case ExpandMode.Default:
                    return source;
                case ExpandMode.Full:
                    return source.Expand(propertySelector);
                case ExpandMode.Shallow:
                    return source.ExpandShallow(propertySelector);
                default:
                    throw new PomonaException("ExpandMode " + expandMode + "not recognized.");
            }
        }


        public static IQueryable<TSource> Expand<TSource, TProperty>(this IQueryable<TSource> source,
                                                                     Expression<Func<TSource, TProperty>>
                                                                         propertySelector,
                                                                     ExpandMode expandMode)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            switch (expandMode)
            {
                case ExpandMode.Default:
                    return source;
                case ExpandMode.Full:
                    return source.Expand(propertySelector);
                case ExpandMode.Shallow:
                    return source.ExpandShallow(propertySelector);
                default:
                    throw new PomonaException("ExpandMode " + expandMode + "not recognized.");
            }
        }


        public static IEnumerable<TSource> Expand<TSource, TProperty>(this IEnumerable<TSource> source,
                                                                      Func<TSource, TProperty> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return source;
        }


        public static IEnumerable<TSource> ExpandShallow<TSource, TProperty>(
            this IEnumerable<TSource> source,
            Func<TSource, TProperty> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return source;
        }


        /// <summary>
        /// Expands as list of references to resources. Only applicable to properties having a collection of resources.
        /// </summary>
        public static IQueryable<TSource> ExpandShallow<TSource, TProperty>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource), typeof(TProperty));
            var property = Expression.Quote(propertySelector);
            var methodCallExpression = Expression.Call(null,
                                                       genericMethod,
                                                       new[]
                                                       {
                                                           source.Expression,
                                                           property
                                                       });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static TSource FirstLazy<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<TSource>(methodCallExpression);
        }


        public static TSource FirstLazy<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.First();
        }


        public static Task<TResult> Future<TSource, TResult>(this IQueryable<TSource> source,
                                                             Expression<Func<IQueryable<TSource>, TResult>> expr)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            var mergedExpression = expr.Body.Replace(expr.Parameters[0], source.Expression);

            return source.Provider.Execute<Task<TResult>>(mergedExpression);
        }


        public static IQueryable<TSource> IncludeTotalCount<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource));
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> IncludeTotalCount<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source;
        }


        public static JToken ToJson<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<JObject>(methodCallExpression);
        }


        public static QueryResult<TSource> ToQueryResult<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression1 = Expression.Call(null, genericMethod, new[] { source.Expression });
            var methodCallExpression = methodCallExpression1;

            return source.Provider.Execute<QueryResult<TSource>>(methodCallExpression);
        }


        public static QueryResult<TSource> ToQueryResult<TSource>(this IEnumerable<TSource> source)
        {
            var enumerable = source as TSource[] ?? source.ToArray();
            return new QueryResult<TSource>(enumerable, 0, enumerable.Length, null, null);
        }


        public static Task<QueryResult<TSource>> ToQueryResultAsync<TSource>(this IQueryable<TSource> source)
        {
            return source.Future(x => x.ToQueryResult());
        }


        public static Uri ToUri<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<Uri>(methodCallExpression);
        }


        public static IQueryable<TSource> WithOptions<TSource>(this IQueryable<TSource> source,
                                                               Action<IRequestOptions> optionsModifier)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource));

            var methodCallExpression = Expression.Call(null,
                                                       genericMethod,
                                                       new[]
                                                       {
                                                           source.Expression, Expression.Constant(optionsModifier)
                                                       });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> WithOptions<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source;
        }
    }
}