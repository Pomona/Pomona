#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.TypeSystem
{
    public static class TypeResolverExtensions
    {
        public static TypeSpec FromType<T>(this ITypeResolver typeResolver)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));
            return typeResolver.FromType(typeof(T));
        }
    }
}