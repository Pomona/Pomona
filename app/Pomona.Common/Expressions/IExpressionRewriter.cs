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