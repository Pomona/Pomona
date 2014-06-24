using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomona.Example.SimpleExtraSite
{
    public class SimplePomonaConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<Type> SourceTypes
        {
            get { return new[]{typeof(SimpleExtraData)}; }
        }

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new SimpleTypeMappingFilter(SourceTypes); }
        }
    }
}
