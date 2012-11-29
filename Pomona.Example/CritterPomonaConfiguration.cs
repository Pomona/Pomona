using System.Collections.Generic;

namespace Pomona.Example
{
    public class CritterPomonaConfiguration : PomonaConfigurationBase
    {
        #region Overrides of PomonaConfigurationBase

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new CritterTypeMappingFilter(); }
        }

        public override IEnumerable<object> FluentRuleObjects
        {
            get { yield return new CritterFluentRules(); }
        }

        #endregion
    }
}