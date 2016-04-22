#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal abstract class PomonaExtendedExpression : Expression
    {
        protected PomonaExtendedExpression(Type type)
        {
            Type = type;
        }


        public abstract ReadOnlyCollection<object> Children { get; }

        public virtual bool LocalExecutionPreferred => false;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public abstract bool SupportedOnServer { get; }

        public override Type Type { get; }
    }
}