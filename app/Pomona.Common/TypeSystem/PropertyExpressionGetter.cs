using System;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    public class PropertyExpressionGetter : PropertyGetter
    {
        public Expression<Func<object, IContainer, object>> Expression { get; }


        internal PropertyExpressionGetter(Expression<Func<object, IContainer, object>> expression)
            : base(expression.Compile())
        {
            this.Expression = expression;
        }
    }
}