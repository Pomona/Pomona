#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface ISerializerNode
    {
        ISerializationContext Context { get; }
        string ExpandPath { get; }
        TypeSpec ExpectedBaseType { get; }
        bool IsRemoved { get; }
        ISerializerNode ParentNode { get; }
        bool SerializeAsReference { get; set; }
        string Uri { get; }
        object Value { get; }
        TypeSpec ValueType { get; }
    }
}