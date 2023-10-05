#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
                throw new ArgumentNullException(nameof(typeResolver));
            return () => new[] { typeResolver.FromType(typeof(T)) };
        }
    }
}
