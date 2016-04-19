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
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    QueryableMethods.GroupBy.MakeGenericMethod(source.ElementType, keySelector.ReturnType),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }


        public static IQueryable OfType(this IQueryable source, Type resultType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.OfType).MakeGenericMethod(resultType),
                    new Expression[] { source.Expression }
                    ));
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

            return (IOrderedQueryable)source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.OrderBy).MakeGenericMethod(source.ElementType, keySelector.ReturnType),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
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

            return (IOrderedQueryable)source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.OrderByDescending).MakeGenericMethod(source.ElementType, keySelector.ReturnType),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }


        public static IQueryable Select(this IQueryable source, LambdaExpression selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.Select).MakeGenericMethod(source.ElementType, selector.ReturnType),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
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
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.SelectMany).MakeGenericMethod(source.ElementType, selectorItemType),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }


        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    QueryableMethods.Skip.MakeGenericMethod(source.ElementType),
                    new Expression[] { source.Expression, Expression.Constant(count) }
                    ));
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
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    QueryableMethods.Take.MakeGenericMethod(source.ElementType),
                    new Expression[] { source.Expression, Expression.Constant(count) }
                    ));
        }


        public static IOrderedQueryable ThenBy(this IOrderedQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return (IOrderedQueryable)source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.ThenBy).MakeGenericMethod(source.ElementType, keySelector.ReturnType),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }


        public static IOrderedQueryable ThenBy(this IOrderedQueryable source,
                                               LambdaExpression keySelector,
                                               SortOrder sortOrder)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return sortOrder == SortOrder.Descending ? source.ThenByDescending(keySelector) : source.ThenBy(keySelector);
        }


        public static IOrderedQueryable ThenByDescending(this IOrderedQueryable source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return (IOrderedQueryable)source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.ThenByDescending).MakeGenericMethod(source.ElementType, keySelector.ReturnType),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }


        public static IQueryable Where(this IQueryable source, LambdaExpression predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    (QueryableMethods.Where).MakeGenericMethod(source.ElementType),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }
    }
}