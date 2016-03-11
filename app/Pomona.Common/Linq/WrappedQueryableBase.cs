#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    public abstract class WrappedQueryableBase<TElement> : IOrderedQueryable<TElement>
    {
        public WrappedQueryableBase(IQueryable<TElement> innerQueryable)
        {
            if (innerQueryable == null)
                throw new ArgumentNullException(nameof(innerQueryable));
            InnerQueryable = innerQueryable;
        }


        protected IQueryable<TElement> InnerQueryable { get; }

        public Type ElementType
        {
            get { return InnerQueryable.ElementType; }
        }

        public Expression Expression
        {
            get { return InnerQueryable.Expression; }
        }


        public IEnumerator<TElement> GetEnumerator()
        {
            return InnerQueryable.GetEnumerator();
        }


        public IQueryProvider Provider
        {
            get { return InnerQueryable.Provider; }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerQueryable.GetEnumerator();
        }
    }
}