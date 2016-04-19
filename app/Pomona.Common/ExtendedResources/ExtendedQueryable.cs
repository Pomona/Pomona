#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Linq;

namespace Pomona.Common.ExtendedResources
{
    public class ExtendedQueryable<T> : QueryableBase<T>
    {
        internal readonly ExtendedQueryProvider provider;


        internal ExtendedQueryable(ExtendedQueryProvider provider, Expression expression)
        {
            this.provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }


        public override Expression Expression { get; }

        public override IQueryProvider Provider => this.provider;
    }
}