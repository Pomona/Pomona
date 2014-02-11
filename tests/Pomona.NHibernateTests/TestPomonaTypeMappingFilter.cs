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

        public TestPomonaTypeMappingFilter(IEnumerable<Type> sourceTypes) : base(sourceTypes)
        {
        }

        #endregion
    }
}