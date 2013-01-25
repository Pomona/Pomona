using System.Collections.Generic;

using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;

namespace Pomona.Example
{
    public class CritterPomonaConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<object> FluentRuleObjects
        {
            get { yield return new CritterFluentRules(); }
        }

        public override ISerializerFactory SerializerFactory
        {
            get { return new PomonaJsonSerializerFactory(); }
        }

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new CritterTypeMappingFilter(); }
        }
    }
}