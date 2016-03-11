#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public static class EnumerableExtensions
    {
        private static readonly MethodInfo castMethod =
            ReflectionHelper.GetMethodDefinition<IEnumerable>(x => x.Cast<object>());

        private static readonly MethodInfo toListMethod =
            ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.ToList());


        public static void AddTo<T>(this IEnumerable<T> source, ICollection<T> target)
        {
            foreach (var item in source)
                target.Add(item);
        }


        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, params T[] value)
        {
            return source.Concat(value);
        }


        public static IEnumerable<T> AppendLazy<T>(this IEnumerable<T> source, Func<T> value)
        {
            return source.Concat(Enumerable.Range(0, 1).Select(x => value()));
        }


        /// <summary>
        /// Cast to IEnumerable{T}, where T will be castType
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="castType">The type of IEnuemerable to cast to.</param>
        /// <returns>Cast IEnumerable.</returns>
        public static IEnumerable Cast(this IEnumerable source, Type castType)
        {
            return (IEnumerable)castMethod.MakeGenericMethod(castType).Invoke(null, new object[] { source });
        }


        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }


        public static IQueryable<T> EmptyIfNull<T>(this IQueryable<T> source)
        {
            return source ?? Enumerable.Empty<T>().AsQueryable();
        }


        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items,
                                                Func<T, IEnumerable<T>> getChildren,
                                                int? maxDepth = null)
        {
            if (maxDepth < 1)
                return Enumerable.Empty<T>();
            return items.SelectMany(x => x.WrapAsEnumerable().Concat(getChildren(x).Flatten(getChildren, maxDepth--)));
        }


        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source.ToList().ForEach(action);
        }


        public static IEnumerable<T> Pad<T>(this IEnumerable<T> source, int count, T paddingValue)
        {
            foreach (var item in source)
            {
                yield return item;
                count--;
            }

            while (count > 0)
            {
                yield return paddingValue;
                count--;
            }
        }


        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty or contains more than one element.
        /// </summary>
        public static TSource SingleOrDefaultIfMultiple<TSource>(this IEnumerable<TSource> source)
        {
            var elements = source.Take(2).ToArray();

            return (elements.Length == 1) ? elements[0] : default(TSource);
        }


        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty or contains more than one element.
        /// </summary>
        public static TSource SingleOrDefaultIfMultiple<TSource>(this IEnumerable<TSource> source,
                                                                 Func<TSource, bool> predicate)
        {
            return source.Where(predicate).SingleOrDefaultIfMultiple();
        }


        public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
            {
                yield return item;
                if (predicate(item))
                    yield break;
            }
        }


        /// <summary>
        /// Version of ToList that detects T of IEnumerable{T} instance.
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>New list.</returns>
        public static IEnumerable ToListDetectType(this IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Type elementType;
            if (!source.GetType().TryGetEnumerableElementType(out elementType))
                return source.Cast<object>().ToList();

            return (IEnumerable)toListMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { source });
        }


        public static IEnumerable<T> WalkTree<T>(this T o, Func<T, T> nextNodeSelector)
            where T : class
        {
            while (o != null)
            {
                yield return o;
                o = nextNodeSelector(o);
            }
        }
    }
}