#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pomona.Common.Expressions
{
    public abstract class ExpressionRewriter : IExpressionRewriter
    {
        public static ExpressionRewriter Create<TExpression>(Func<IRewriteContext, TExpression, Expression> func)
            where TExpression : Expression
        {
            return new LambdaRewriter<TExpression>(func);
        }


        public static ExpressionRewriter Create(Func<IRewriteContext, Expression, Expression> func)
        {
            return Create<Expression>(func);
        }


        public static ExpressionRewriter Create(Func<Expression, Expression> func)
        {
            return Create<Expression>(func);
        }


        public static ExpressionRewriter Create<TExpression>(Func<TExpression, Expression> func)
            where TExpression : Expression
        {
            return new LambdaRewriter<TExpression>(func);
        }


        internal abstract Expression OnVisit(IRewriteContext context, Expression node);
        public abstract IEnumerable<Type> VisitedTypes { get; }


        Expression IExpressionRewriter.Visit(IRewriteContext context, Expression node)
        {
            return OnVisit(context, node);
        }
    }

    public abstract class ExpressionRewriter<TExpression> : ExpressionRewriter
        where TExpression : Expression
    {
        private static readonly IEnumerable<Type> visitedTypes =
            new ReadOnlyCollection<Type>(new[] { typeof(TExpression) });

        public override IEnumerable<Type> VisitedTypes => visitedTypes;

        public abstract Expression Visit(IRewriteContext context, TExpression node);


        internal override Expression OnVisit(IRewriteContext context, Expression node)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            var castExpression = node as TExpression;
            return castExpression != null ? Visit(context, castExpression) : node;
        }
    }
}

