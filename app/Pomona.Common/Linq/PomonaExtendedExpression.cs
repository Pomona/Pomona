using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal abstract class PomonaExtendedExpression : Expression
    {
        private readonly Type type;

        public virtual bool  LocalExecutionPreferred { get { return false; } }

        protected PomonaExtendedExpression(Type type)
        {
            this.type = type;
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }


        public override Type Type
        {
            get { return this.type; }
        }

        public abstract bool SupportedOnServer { get; }

        public abstract ReadOnlyCollection<object> Children { get; }

    }
}