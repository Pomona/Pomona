using System;
using System.Linq;
using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ServerDeserializationContext : IDeserializationContext
    {
        private readonly PomonaSession pomonaSession;

        public ServerDeserializationContext(PomonaSession pomonaSession)
        {
            this.pomonaSession = pomonaSession;
        }

        public object CreateReference(IMappedType type, string uri)
        {
            return pomonaSession.GetObjectFromUri(uri);
        }

        public void Deserialize<TReader>(IDeserializerNode node, IDeserializer<TReader> deserializer, TReader reader) where TReader : ISerializerReader
        {
            deserializer.DeserializeNode(node, reader);
        }

        public IMappedType GetTypeByName(string typeName)
        {
            TransformedType transformedType = pomonaSession.TypeMapper.TransformedTypes.FirstOrDefault(x => x.Name == typeName);
            if (transformedType == null)
                throw new PomonaSerializationException("Can't deserialize object of unrecognized type " + typeName);
            return transformedType;
        }
    }
}