#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
    }
}