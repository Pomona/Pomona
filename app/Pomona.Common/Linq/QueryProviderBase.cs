#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    public abstract class QueryProviderBase : IQueryProvider
    {
        private static readonly Func<Type, QueryProviderBase, Expression, IQueryable> createQueryGeneric;


        static QueryProviderBase()
        {
            createQueryGeneric = GenericInvoker
                .Instance<QueryProviderBase>()
                .CreateFunc1<Expression, IQueryable>(x => x.CreateQuery<object>(null));
        }


        public abstract object Execute(Expression expression, Type returnType);


        private static Type GetElementType(Type type)
        {
            var queryableTypeInstance = type.GetInterfacesOfGeneric(typeof(IQueryable<>)).FirstOrDefault();
            if (queryableTypeInstance == null)
                return type;

            return queryableTypeInstance.GetGenericArguments()[0];
        }


        public abstract IQueryable<TElement> CreateQuery<TElement>(Expression expression);


        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            return createQueryGeneric(GetElementType(expression.Type), this, expression);
        }


        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression, typeof(TResult));
        }


        object IQueryProvider.Execute(Expression expression)
        {
            var exprType = expression.Type;
            Type[] genargs;
            if (exprType.TryExtractTypeArguments(typeof(IOrderedQueryable<>), out genargs))
                exprType = typeof(IOrderedEnumerable<>).MakeGenericType(genargs);
            else if (exprType.TryExtractTypeArguments(typeof(IQueryable<>), out genargs))
                exprType = typeof(IEnumerable<>).MakeGenericType(genargs);
            return Execute(expression, exprType);
        }
    }
}

