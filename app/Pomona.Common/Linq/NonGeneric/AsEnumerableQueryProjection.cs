#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.NonGeneric
{
    internal class AsEnumerableQueryProjection : QueryProjection
    {
        public override string Name => "AsEnumerable";


        public override Expression Apply(IQueryable queryable)
        {
            return queryable.Expression;
        }


        public override object Execute(IQueryable queryable)
        {
            return queryable;
        }


        public override Type GetResultType(Type elementType)
        {
            return elementType;
        }
    }
}