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
    public abstract class QueryableBase<T> : IOrderedQueryable<T>
    {
        public override string ToString()
        {
            if (Expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)Expression).Value == this)
                return "Query(" + typeof(T) + ")";
            return Expression.ToString();
        }


        public abstract Expression Expression { get; }


        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }


        public abstract IQueryProvider Provider { get; }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }
    }
}