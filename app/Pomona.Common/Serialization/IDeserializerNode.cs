#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface IDeserializerNode : IResourceNode
    {
        IDeserializationContext Context { get; }
        string ExpandPath { get; }
        TypeSpec ExpectedBaseType { get; }
        DeserializerNodeOperation Operation { get; set; }
        new IDeserializerNode Parent { get; }
        string Uri { get; set; }
        new object Value { get; set; }
        TypeSpec ValueType { get; }
        void CheckAccessRights(HttpMethod method);
        void CheckItemAccessRights(HttpMethod method);
        void SetProperty(PropertySpec property, object propertyValue);
        void SetValueType(string typeName);
        void SetValueType(Type type);
    }
}