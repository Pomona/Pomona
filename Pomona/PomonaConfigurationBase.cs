using System.Collections.Generic;

using Pomona.Common.Serialization;

namespace Pomona
{
    public abstract class PomonaConfigurationBase
    {
        public abstract IEnumerable<object> FluentRuleObjects { get; }
        public abstract ISerializerFactory SerializerFactory { get; }
        public abstract ITypeMappingFilter TypeMappingFilter { get; }
    }
}