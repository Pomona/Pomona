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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.NonGeneric
{
    public sealed class QueryableProjection
    {
        public static readonly QueryableProjection Any = Create(x => x.Any());
        public static readonly QueryableProjection Count = Create(x => x.Count());
        public static readonly QueryableProjection First = Create(x => x.First());
        public static readonly QueryableProjection FirstOrDefault = Create(x => x.FirstOrDefault());
        public static readonly QueryableProjection Max = Create(x => x.Max());
        public static readonly QueryableProjection Min = Create(x => x.Min());
        public static readonly QueryableProjection Single = Create(x => x.Single());
        public static readonly QueryableProjection SingleOrDefault = Create(x => x.SingleOrDefault());
        public static readonly QueryableProjection Sum = CreateNonGeneric("Sum");
        private readonly string name;
        private readonly Func<IQueryable, Expression> projectionFunc;


        private QueryableProjection(Func<IQueryable, Expression> projectionFunc, string name)
        {
            this.projectionFunc = projectionFunc;
            this.name = name;
        }


        public override string ToString()
        {
            return this.name;
        }


        public object Execute(IQueryable queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException("queryable");
            return queryable.Provider.Execute(this.projectionFunc(queryable));
        }


        public object Execute<T>(IQueryable<T> queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException("queryable");
            return queryable.Provider.Execute(this.projectionFunc(queryable));
        }


        private static Expression ApplyToNonGenericMethod(IQueryable source, string name)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var iqType = typeof(IQueryable<>).MakeGenericType(source.ElementType);
            var method = typeof(Queryable).GetMethod(name,
                                                     BindingFlags.Public | BindingFlags.Static,
                                                     null,
                                                     new Type[] { iqType },
                                                     null);
            if (method == null)
                throw new NotSupportedException("Unable to apply " + name + " to " + iqType);
            return Expression.Call(null, method, new Expression[] { source.Expression });
        }


        private static QueryableProjection Create(Expression<Func<IQueryable<object>, object>> func, string name = null)
        {
            var method = func.ExtractMethodInfo();
            return
                new QueryableProjection(
                    source =>
                        Expression.Call(null,
                                        method.MakeGenericMethod(source.ElementType),
                                        new Expression[] { source.Expression }),
                    name ?? method.Name);
        }


        private static QueryableProjection CreateNonGeneric(string name)
        {
            return new QueryableProjection(q => ApplyToNonGenericMethod(q, name), name);
        }
    }
}