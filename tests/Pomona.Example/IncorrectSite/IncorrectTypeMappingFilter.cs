#region License
// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/
#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Example.IncorrectSite
{
    public class IncorrectTypeMappingFilter : TypeMappingFilterBase
    {
        public IncorrectTypeMappingFilter(IEnumerable<Type> sourceTypes)
            : base(sourceTypes)
        {
        }

        public override ClientMetadata ClientMetadata => base.ClientMetadata.With("Incorrect.Client", "IncorrectClient", "IIncorrectClient", "Incorrect.Client");
    }
}

