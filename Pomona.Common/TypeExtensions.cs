using System;
using System.Linq;

namespace Pomona.Common
{
    public static class TypeExtensions
    {
        public static bool IsGenericInstanceOf(this Type type, params Type[] genericTypeDefinitions)
        {
            return genericTypeDefinitions.Any(x => x.MetadataToken == type.MetadataToken);
        }
    }
}