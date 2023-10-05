#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface ISerializationContext : IContainer
    {
        TypeSpec GetClassMapping(Type type);
        string GetUri(object value);
        string GetUri(PropertySpec property, object value);
        bool PathToBeExpanded(string expandPath);
        void Serialize(ISerializerNode node, Action<ISerializerNode> serializeNodeAction);
    }
}

