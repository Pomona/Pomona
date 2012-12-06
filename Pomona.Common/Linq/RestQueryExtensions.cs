using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Linq
{
    public static class RestQueryExtensions
    {
        public static IQueryable<TSource> Expand<TSource, TProperty>(this IQueryable<TSource> source,
                                                                     Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertySelector == null) throw new ArgumentNullException("propertySelector");
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TProperty)),
                    new[] { source.Expression, Expression.Quote(propertySelector) }
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