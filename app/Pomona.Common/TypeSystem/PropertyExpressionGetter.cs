#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    public class PropertyExpressionGetter : PropertyGetter
    {
        internal PropertyExpressionGetter(Expression<Func<object, IContainer, object>> expression)
            : base(expression.Compile())
        {
            Expression = expression;
        }


        public Expression<Func<object, IContainer, object>> Expression { get; }
    }
}