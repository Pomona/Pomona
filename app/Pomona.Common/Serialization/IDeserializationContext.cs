#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface IDeserializationContext : IContainer
    {
        IResourceNode TargetNode { get; }
        void CheckAccessRights(PropertySpec property, HttpMethod method);
        void CheckPropertyItemAccessRights(PropertySpec property, HttpMethod method);
        object CreateReference(IDeserializerNode node);
        object CreateResource(TypeSpec type, IConstructorPropertySource args);
        void Deserialize(IDeserializerNode node, Action<IDeserializerNode> deserializeNodeAction);
        TypeSpec GetClassMapping(Type type);
        TypeSpec GetTypeByName(string typeName);
        void OnMissingRequiredPropertyError(IDeserializerNode node, PropertySpec targetProp);
        void SetProperty(IDeserializerNode target, PropertySpec property, object propertyValue);
    }
}

