#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona.Common.Expressions
{
    public interface IExpressionRewriter
    {
        IEnumerable<Type> VisitedTypes { get; }
        Expression Visit(IRewriteContext context, Expression node);
    }
}