#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    public class RestQuery<T> : QueryableBase<T>
    {
        private readonly RestQueryProvider provider;


        protected RestQuery(RestQueryProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            this.provider = provider;
            Expression = Expression.Constant(this);
        }


        public RestQuery(RestQueryProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException(nameof(expression));
            this.provider = provider;
            Expression = expression;
        }


        public override Expression Expression { get; }

        public override IQueryProvider Provider => this.provider;
    }
}

