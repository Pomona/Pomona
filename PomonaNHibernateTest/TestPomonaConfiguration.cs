using System.Collections.Generic;
using Pomona;

namespace PomonaNHibernateTest
{
    public class TestPomonaConfiguration : PomonaConfigurationBase
    {
        #region Overrides of PomonaConfigurationBase

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new TestPomonaTypeMappingFilter(); }
        }

        public override IEnumerable<object> FluentRuleObjects
        {
            get { yield break; }
        }

        #endregion
    }
}