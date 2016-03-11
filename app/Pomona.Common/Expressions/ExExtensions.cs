#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Linq.NonGeneric;

namespace Pomona.Common.Expressions
{
    public static class ExExtensions
    {
        public static IQueryable SelectEx(this IQueryable source, Func<Ex, Ex> selector)
        {
            return source.Select(Ex.Lambda(source.ElementType, selector));
        }


        public static IQueryable SelectManyEx(this IQueryable source, Func<Ex, Ex> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            var param = Expression.Parameter(source.ElementType);
            Expression body = selector(param);
            Type elementType;
            if (!body.Type.TryGetEnumerableElementType(out elementType))
                throw new ArgumentException("selector must return an IEnumerable<T>", nameof(selector));

            var delType = Expression.GetFuncType(source.ElementType, typeof(IEnumerable<>).MakeGenericType(elementType));
            return source.SelectMany(Expression.Lambda(delType, body, param));
        }


        public static IQueryable<T> WhereEx<T>(this IQueryable<T> source, Func<Ex, Ex> func)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            return (IQueryable<T>)source.Where(Ex.Lambda(typeof(T), func));
        }


        public static IQueryable WhereEx(this IQueryable source, Func<Ex, Ex> func)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            return source.Where(Ex.Lambda(source.ElementType, func));
        }
    }
}