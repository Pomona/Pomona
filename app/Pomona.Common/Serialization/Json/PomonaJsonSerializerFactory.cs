#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.Serialization.Json
{
    public class PomonaJsonSerializerFactory : TextSerializerFactoryBase<PomonaJsonSerializer, PomonaJsonDeserializer>
    {
        public override PomonaJsonDeserializer GetDeserializer(ISerializationContextProvider contextProvider)
        {
            return new PomonaJsonDeserializer(contextProvider);
        }


        public override PomonaJsonSerializer GetSerializer(ISerializationContextProvider contextProvider)
        {
            return new PomonaJsonSerializer(contextProvider);
        }
    }
}