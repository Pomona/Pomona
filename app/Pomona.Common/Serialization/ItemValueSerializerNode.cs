#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ItemValueSerializerNode : SerializerNode
    {
        public ItemValueSerializerNode(object value,
                                       TypeSpec expectedBaseType,
                                       string expandPath,
                                       ISerializationContext context,
                                       ISerializerNode parentNode,
                                       bool isRemoved = false)
            : base(expectedBaseType, expandPath, context, parentNode, isRemoved)
        {
            Value = value;
        }


        public override object Value { get; }
    }
}

