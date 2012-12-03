using System.Collections.Generic;

namespace Pomona
{
    public abstract class PomonaConfigurationBase
    {
        public abstract ITypeMappingFilter TypeMappingFilter { get; }
        public abstract IEnumerable<object> FluentRuleObjects { get; }
    }
}