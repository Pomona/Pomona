#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyFormulaAttribute : Attribute
    {
        public PropertyFormulaAttribute(LambdaExpression formula)
        {
            if (formula == null)
                throw new ArgumentNullException(nameof(formula));
            Formula = formula;
        }


        public LambdaExpression Formula { get; }
    }
}
