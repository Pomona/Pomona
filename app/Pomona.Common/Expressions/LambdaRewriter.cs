#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.Expressions
{
    public class LambdaRewriter<TExpression> : ExpressionRewriter<TExpression>
        where TExpression : Expression
    {
        private readonly Func<IRewriteContext, TExpression, Expression> visitMethod;


        public LambdaRewriter(Func<TExpression, Expression> visitMethod)
            : this(
                visitMethod != null
                    ? (Func<IRewriteContext, TExpression, Expression>)((v, expr) => visitMethod(expr))
                    : null)
        {
        }


        public LambdaRewriter(Func<IRewriteContext, TExpression, Expression> visitMethod)
        {
            if (visitMethod == null)
                throw new ArgumentNullException(nameof(visitMethod));
            this.visitMethod = visitMethod;
        }


        public override Expression Visit(IRewriteContext context, TExpression node)
        {
            return this.visitMethod(context, node);
        }
    }
}

