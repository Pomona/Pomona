#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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

        public override IEnumerable<Type> VisitedTypes
        {
            get { return visitedTypes; }
        }

        public abstract Expression Visit(IRewriteContext context, TExpression node);


        internal override Expression OnVisit(IRewriteContext context, Expression node)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            var castExpression = node as TExpression;
            return castExpression != null ? Visit(context, castExpression) : node;
        }
    }
}