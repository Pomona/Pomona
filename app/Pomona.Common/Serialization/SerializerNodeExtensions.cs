namespace Pomona.Common.Serialization
{
    public static class SerializerNodeExtensions
    {
        public static void Serialize<TWriter>(this ISerializerNode node, ISerializer<TWriter> serializer, TWriter writer)
            where TWriter : ISerializerWriter
        {
            node.Context.Serialize(node, serializer, writer);
        }
    }
}