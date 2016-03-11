#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.NonGeneric
{
    public abstract class QueryProjection
    {
        public static readonly QueryProjection Any = Create(x => x.Any());
        public static readonly QueryProjection Count = Create(x => x.Count());
        public static readonly QueryProjection AsEnumerable = new AsEnumerableQueryProjection();
        public static readonly QueryProjection First = Create(x => x.First());
        public static readonly QueryProjection FirstOrDefault = Create(x => x.FirstOrDefault());
        public static readonly QueryProjection Last = Create(x => x.Last());
        public static readonly QueryProjection LastOrDefault = Create(x => x.LastOrDefault());
        public static readonly QueryProjection Max = Create(x => x.Max());
        public static readonly QueryProjection Min = Create(x => x.Min());
        public static readonly QueryProjection Single = Create(x => x.Single());
        public static readonly QueryProjection SingleOrDefault = Create(x => x.SingleOrDefault());
        public static readonly QueryProjection Sum = CreateNonGeneric("Sum");


        internal QueryProjection()
        {
        }


        public abstract string Name { get; }
        public abstract Expression Apply(IQueryable queryable);


        public virtual object Execute(IQueryable queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            return queryable.Provider.Execute(Apply(queryable));
        }


        public T Execute<T>(IQueryable queryable)
        {
            return (T)Execute(queryable);
        }


        /// <summary>
        /// Gets the resulting type given an IQueryable with specified element type.
        /// </summary>
        /// <param name="elementType">The element type</param>
        /// <returns>The type the projection would produce.</returns>
        public abstract Type GetResultType(Type elementType);


        public override string ToString()
        {
            return Name;
        }


        private static QueryProjection Create(Expression<Func<IQueryable<object>, object>> func, string name = null)
        {
            var method = func.ExtractMethodInfo();
            return
                new QueryProjectionUsingGenericMethod(method, name ?? method.Name);
        }


        private static QueryProjection CreateNonGeneric(string name)
        {
            return new QueryProjectionUsingNonGenericMethod(name);
        }
    }
}