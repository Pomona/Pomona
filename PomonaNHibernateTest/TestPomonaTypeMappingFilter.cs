using System;
using System.Collections.Generic;
using System.Linq;

using Pomona;

using PomonaNHibernateTest.Models;

namespace PomonaNHibernateTest
{
    public class TestPomonaTypeMappingFilter : TypeMappingFilterBase
    {
        #region Overrides of TypeMappingFilterBase

        public override object GetIdFor(object entity)
        {
            return ((EntityBase) entity).Id;
        }


        public override IEnumerable<Type> GetSourceTypes()
        {
            return typeof (EntityBase).Assembly.GetTypes().Where(x => typeof (EntityBase).IsAssignableFrom(x) && x == typeof(EntityAttribute));
        }

        #endregion
    }
}