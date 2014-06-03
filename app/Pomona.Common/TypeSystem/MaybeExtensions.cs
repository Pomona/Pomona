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