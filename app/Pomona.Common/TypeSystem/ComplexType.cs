#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public class ComplexType : StructuredType
    {
        public ComplexType(IStructuredTypeResolver typeResolver,
                           Type type,
                           Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
        }
    }
}

