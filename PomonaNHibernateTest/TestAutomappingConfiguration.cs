using System;

using FluentNHibernate.Automapping;

using PomonaNHibernateTest.Models;

namespace PomonaNHibernateTest
{
    public class TestAutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            return typeof (EntityBase).IsAssignableFrom(type) || type == typeof(EntityAttribute);
        }
    }
}