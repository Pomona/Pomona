using System;
using System.Collections.Generic;
using System.Linq;
using Pomona;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using PomonaNHibernateTest.Models;

namespace PomonaNHibernateTest
{
    public class TestPomonaConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<Type> SourceTypes
        {
            get
            {
                return typeof (EntityBase).Assembly.GetTypes()
                    .Where(x => typeof (EntityBase).IsAssignableFrom(x) || x == typeof (EntityAttribute));
            }
        }

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
            get
            {
                return new TestPomonaTypeMappingFilter(SourceTypes);;
            }
        }
    }
}