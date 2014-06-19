using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomona.Example.SimpleExtraSite
{
    public class SimpleTypeMappingFilter:TypeMappingFilterBase
    {
        public SimpleTypeMappingFilter(IEnumerable<Type> sourceTypes)
            : base(sourceTypes)
        {

        }
        public override string GetClientAssemblyName()
        {
            return "Extra.Client";
        }

        public override bool TypeIsMapped(Type type)
        {
            return type==typeof(SimpleExtraData)||base.TypeIsMapped(type);
        }
    }
}
