#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries.Interception
{
    public static class InterceptedQueryable
    {
        public static IQueryable<T> CreateLazy<T>(Func<Type, IQueryable> queryableFactory, IEnumerable<ExpressionVisitor> visitors)
        {
            if (queryableFactory == null)
                throw new ArgumentNullException(nameof(queryableFactory));
            if (visitors == null)
                throw new ArgumentNullException(nameof(visitors));
            return new InterceptedQueryProvider(visitors).CreateLazySource<T>(queryableFactory);
        }
    }

    public class InterceptedQueryable<T> : QueryableBase<T>
    {
        private readonly InterceptedQueryProvider provider;


        internal InterceptedQueryable(InterceptedQueryProvider provider, Expression expression)
        {
            this.provider = provider;
            Expression = expression ?? Expression.Constant(this, typeof(IQueryable<T>));
        }


        public override Expression Expression { get; }

        public override IQueryProvider Provider
        {
            get { return this.provider; }
        }
    }
}