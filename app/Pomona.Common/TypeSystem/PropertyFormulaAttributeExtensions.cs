#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    public static class PropertyFormulaAttributeExtensions
    {
        public static LambdaExpression GetPropertyFormula(this PropertySpec property)
        {
            return
                MaybeExtensions.MaybeFirst<PropertyFormulaAttribute>(property.Attributes.OfType<PropertyFormulaAttribute>())
                               .Select(x => x.Formula)
                               .OrDefault();
        }
    }
}