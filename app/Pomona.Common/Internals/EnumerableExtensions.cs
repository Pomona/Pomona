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


        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params T[] value)
        {
            return source.Concat((IEnumerable<T>)value);
        }


        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }


        public static IQueryable<T> EmptyIfNull<T>(this IQueryable<T> source)
        {
            return source ?? Enumerable.Empty<T>().AsQueryable();
        }


        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> getChildren)
        {
            return items.SelectMany(x => x.WrapAsEnumerable().Concat(getChildren(x).Flatten(getChildren)));
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
                throw new ArgumentNullException("source");
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