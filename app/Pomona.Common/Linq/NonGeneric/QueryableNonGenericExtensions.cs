#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.NonGeneric
{
    public static class QueryableNonGenericExtensions
    {
        public static object Execute(this IQueryable source, QueryProjection projection)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (projection == null)
                throw new ArgumentNullException(nameof(projection));

            return projection.Execute(source);
        }


        public static IQueryable GroupBy(this IQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var method = QueryableMethods.GroupBy.MakeGenericMethod(source.ElementType, keySelector.ReturnType);
            var quotedKeySelector = Expression.Quote(keySelector);
            var arguments = new[] { source.Expression, quotedKeySelector };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }


        public static IQueryable OfType(this IQueryable source, Type resultType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            var method = QueryableMethods.OfType.MakeGenericMethod(resultType);
            var arguments = source.Expression;
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }


        public static IQueryable OfTypeIfRequired(this IQueryable source, Type resultType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            if (source.ElementType == resultType)
                return source;

            return OfType(source, resultType);
        }


        public static IOrderedQueryable OrderBy(this IQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var method = QueryableMethods.OrderBy.MakeGenericMethod(source.ElementType, keySelector.ReturnType);
            var quotedKeySelector = Expression.Quote(keySelector);
            var arguments = new Expression[] { source.Expression, quotedKeySelector };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return (IOrderedQueryable)source.Provider.CreateQuery(methodCallExpression);
        }


        public static IOrderedQueryable OrderBy(this IQueryable source,
                                                LambdaExpression keySelector,
                                                SortOrder sortOrder)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return sortOrder == SortOrder.Descending
                ? source.OrderByDescending(keySelector)
                : source.OrderBy(keySelector);
        }


        public static IOrderedQueryable OrderByDescending(this IQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var method = QueryableMethods.OrderByDescending.MakeGenericMethod(source.ElementType, keySelector.ReturnType);
            var quotedKeySelector = Expression.Quote(keySelector);
            var arguments = new[] { source.Expression, quotedKeySelector };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return (IOrderedQueryable)source.Provider.CreateQuery(methodCallExpression);
        }


        public static IQueryable Select(this IQueryable source, LambdaExpression selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var quotedSelector = Expression.Quote(selector);
            var arguments = new[] { source.Expression, quotedSelector };
            var method = QueryableMethods.Select.MakeGenericMethod(source.ElementType, selector.ReturnType);
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }


        public static IQueryable SelectMany(this IQueryable source, LambdaExpression selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            Type selectorItemType;
            if (!selector.ReturnType.TryGetEnumerableElementType(out selectorItemType))
                throw new ArgumentException("The return type of selector is not an IEnumerable<T>", nameof(selector));

            var method = QueryableMethods.SelectMany.MakeGenericMethod(source.ElementType, selectorItemType);
            var quotedSelector = Expression.Quote(selector);
            var arguments = new[] { source.Expression, quotedSelector };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }


        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var countExpression = Expression.Constant(count);
            var arguments = new[] { source.Expression, countExpression };
            var method = QueryableMethods.Skip.MakeGenericMethod(source.ElementType);
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }


        public static object Sum(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var iqType = typeof(IQueryable<>).MakeGenericType(source.ElementType);
            var method = typeof(Queryable).GetMethod("Sum",
                                                     BindingFlags.Public | BindingFlags.Static,
                                                     null,
                                                     new Type[] { iqType },
                                                     null);
            if (method == null)
                throw new NotSupportedException("Unable to apply Sum to " + iqType);

            return method.Invoke(null, new object[] { source });
        }


        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var method = QueryableMethods.Take.MakeGenericMethod(source.ElementType);
            var countExpression = Expression.Constant(count);
            var arguments = new[] { source.Expression, countExpression };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }


        public static IOrderedQueryable ThenBy(this IOrderedQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var arguments = new[] { source.Expression, Expression.Quote(keySelector) };
            var method = QueryableMethods.ThenBy.MakeGenericMethod(source.ElementType, keySelector.ReturnType);
            var methodCallExpression = Expression.Call(null, method, arguments);
            return (IOrderedQueryable)source.Provider.CreateQuery(methodCallExpression);
        }


        public static IOrderedQueryable ThenBy(this IOrderedQueryable source,
                                               LambdaExpression keySelector,
                                               SortOrder sortOrder)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return sortOrder == SortOrder.Descending
                ? source.ThenByDescending(keySelector)
                : source.ThenBy(keySelector);
        }


        public static IOrderedQueryable ThenByDescending(this IOrderedQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var method = QueryableMethods.ThenByDescending.MakeGenericMethod(source.ElementType, keySelector.ReturnType);
            var quotedKeySelector = Expression.Quote(keySelector);
            var arguments = new[] { source.Expression, quotedKeySelector };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return (IOrderedQueryable)source.Provider.CreateQuery(methodCallExpression);
        }


        public static IQueryable Where(this IQueryable source, LambdaExpression predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var method = QueryableMethods.Where.MakeGenericMethod(source.ElementType);
            var quotedPredicate = Expression.Quote(predicate);
            var arguments = new[] { source.Expression, quotedPredicate };
            var methodCallExpression = Expression.Call(null, method, arguments);
            return source.Provider.CreateQuery(methodCallExpression);
        }
    }
}