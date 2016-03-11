#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries.Interception
{
    public class QueryExecutingEventArgs : EventArgs
    {
        public QueryExecutingEventArgs(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            Expression = expression;
        }


        public Expression Expression { get; }
    }
}