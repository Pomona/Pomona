#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Common.TypeSystem
{
    public interface ITypeFactory
    {
        int Priority { get; }
        TypeSpec CreateFromType(ITypeResolver typeResolver, Type type);
    }
}

