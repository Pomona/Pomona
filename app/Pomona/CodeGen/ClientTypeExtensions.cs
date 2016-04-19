#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.CodeGen
{
    public static class ClientTypeExtensions
    {
        public static Type GetCustomClientLibraryType(this TypeSpec type)
        {
            return
                type.DeclaredAttributes.OfType<CustomClientLibraryTypeAttribute>().MaybeFirst().Select(x => x.Type).OrDefault();
        }
    }
}