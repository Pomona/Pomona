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
                throw new ArgumentNullException("source");
            if (selector == null)
                throw new ArgumentNullException("selector");
            var param = Expression.Parameter(source.ElementType);
            Expression body = selector(param);
            Type elementType;
            if (!body.Type.TryGetEnumerableElementType(out elementType))
                throw new ArgumentException("selector must return an IEnumerable<T>", "selector");

            var delType = Expression.GetFuncType(source.ElementType, typeof(IEnumerable<>).MakeGenericType(elementType));
            return source.SelectMany(Expression.Lambda(delType, body, param));
        }


        public static IQueryable<T> WhereEx<T>(this IQueryable<T> source, Func<Ex, Ex> func)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (func == null)
                throw new ArgumentNullException("func");
            return (IQueryable<T>)source.Where(Ex.Lambda(typeof(T), func));
        }


        public static IQueryable WhereEx(this IQueryable source, Func<Ex, Ex> func)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (func == null)
                throw new ArgumentNullException("func");
            return source.Where(Ex.Lambda(source.ElementType, func));
        }
    }
}