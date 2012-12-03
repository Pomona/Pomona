namespace Pomona.Common.Serialization
{
    public static class DeserializerNodeExtensions
    {
        public static void Deserialize<TReader>(
            this IDeserializerNode node, IDeserializer<TReader> deserializer, TReader reader)
            where TReader : ISerializerReader
        {
            node.Context.Deserialize(node, deserializer, reader);
        }
    }
}