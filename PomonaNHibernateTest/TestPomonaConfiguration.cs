using System.Collections.Generic;

using Pomona;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;

namespace PomonaNHibernateTest
{
    public class TestPomonaConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<object> FluentRuleObjects
        {
            get { yield break; }
        }

        public override ISerializerFactory SerializerFactory
        {
            get { return new PomonaJsonSerializerFactory(); }
        }

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new TestPomonaTypeMappingFilter(); }
        }
    }
}