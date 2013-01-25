using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface IDeserializationContext
    {
        object CreateReference(IMappedType type, string uri);


        void Deserialize<TReader>(IDeserializerNode node, IDeserializer<TReader> deserializer, TReader reader)
            where TReader : ISerializerReader;


        IMappedType GetTypeByName(string typeName);
    }
}