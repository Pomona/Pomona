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
using System.Reflection;

namespace Pomona.Common.Linq.NonGeneric
{
    internal abstract class QueryProjectionMethodBase : QueryProjection
    {
        public override Expression Apply(IQueryable queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            var method = GetMethod(queryable.ElementType);
            return Expression.Call(method, queryable.Expression);
        }


        public override Type GetResultType(Type elementType)
        {
            return GetMethod(elementType).ReturnType;
        }


        protected abstract MethodInfo GetMethod(Type elementType);


        private static MethodInfo GetNonGenericQueryableMethod(Type iqType, string name)
        {
            var method = typeof(Queryable).GetMethod(name,
                                                     BindingFlags.Public | BindingFlags.Static,
                                                     null,
                                                     new Type[] { iqType },
                                                     null);
            if (method == null)
                throw new NotSupportedException("Unable to apply " + name + " to " + iqType);
            return method;
        }
    }
}