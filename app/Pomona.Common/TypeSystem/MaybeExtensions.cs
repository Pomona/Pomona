#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public static class MaybeExtensions
    {
        public static Maybe<T> Maybe<T>(this T val, Func<T, bool> predicate)
            where T : class
        {
            return ReferenceEquals(val, null) ? TypeSystem.Maybe<T>.Empty : new Maybe<T>(val).Where(predicate);
        }


        public static Maybe<T> Maybe<T>(this T? val, Func<T, bool> predicate)
            where T : struct
        {
            return val.HasValue ? new Maybe<T>(val.Value).Where(predicate) : TypeSystem.Maybe<T>.Empty;
        }


        public static Maybe<T> Maybe<T>(this T val)
            where T : class
        {
            return ReferenceEquals(val, null) ? TypeSystem.Maybe<T>.Empty : new Maybe<T>(val);
        }


        public static Maybe<T> Maybe<T>(this T? val)
            where T : struct
        {
            return val.HasValue ? new Maybe<T>(val.Value) : TypeSystem.Maybe<T>.Empty;
        }


        public static Maybe<T> MaybeAs<T>(this object val)
        {
            return val.Maybe().OfType<T>();
        }


        public static Maybe<T> MaybeFirst<T>(this IEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    return new Maybe<T>(enumerator.Current);
            }
            return TypeSystem.Maybe<T>.Empty;
        }
    }
}