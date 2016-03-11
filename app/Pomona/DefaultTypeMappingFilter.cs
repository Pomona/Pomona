#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona
{
    public class DefaultTypeMappingFilter : TypeMappingFilterBase
    {
        public DefaultTypeMappingFilter(IEnumerable<Type> sourceTypes)
            : base(sourceTypes)
        {
        }
    }
}