#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Example.SimpleExtraSite
{
    public class SimpleTypeMappingFilter : TypeMappingFilterBase
    {
        public SimpleTypeMappingFilter(IEnumerable<Type> sourceTypes)
            : base(sourceTypes)
        {
        }


        public override ClientMetadata ClientMetadata => base.ClientMetadata.With("Extra.Client", "ExtraClient", "IExtraClient", "Extra.Client");


        public override bool GenerateIndependentClient()
        {
            return false;
        }


        public override bool TypeIsMapped(Type type)
        {
            return type == typeof(SimpleExtraData) || base.TypeIsMapped(type);
        }
    }
}

