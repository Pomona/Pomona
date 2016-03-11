#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Queries
{
    public interface IQueryTypeResolver
    {
        Type ResolveType(string typeName);


        bool TryResolveProperty<TProperty>(Type type, string propertyPath, out TProperty property)
            where TProperty : PropertySpec;
    }
}