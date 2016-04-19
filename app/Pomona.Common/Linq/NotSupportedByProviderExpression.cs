#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal class NotSupportedByProviderExpression : PomonaExtendedExpression
    {
        public NotSupportedByProviderExpression(Expression expression, Exception exception = null)
            : base(expression.Type)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            Expression = expression;
            Exception = exception;
        }


        public override ReadOnlyCollection<object> Children => new ReadOnlyCollection<object>(new object[] { });

        public Exception Exception { get; }

        public Expression Expression { get; }

        public override bool LocalExecutionPreferred => true;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool SupportedOnServer => false;


        public override string ToString()
        {
            return "(☹ node \"" + Expression + "\" not supported. ]])";
        }
    }
}