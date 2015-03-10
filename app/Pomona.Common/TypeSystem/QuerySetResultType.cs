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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    internal class QuerySetResultType<T> : QueryResultType
    {
        public QuerySetResultType(IStructuredTypeResolver typeResolver)
            : base(typeResolver, typeof(QuerySetResult<T>), GetGenericArguments(typeResolver))
        {
        }


        protected internal override ConstructorSpec OnLoadConstructor()
        {
            Expression<Func<IConstructorControl<QuerySetResult<T>>, QuerySetResult<T>>> expr =
                x =>
                    new QuerySetResult<T>(x.Requires().Items, x.Optional().Skip, x.Optional().TotalCount,
                                          x.Optional().Previous, x.Optional().Next);
            return new ConstructorSpec(expr);
        }


        private static Func<IEnumerable<TypeSpec>> GetGenericArguments(ITypeResolver typeResolver)
        {
            if (typeResolver == null)
                throw new ArgumentNullException("typeResolver");
            return () => new[] { typeResolver.FromType(typeof(T)) };
        }
    }
}