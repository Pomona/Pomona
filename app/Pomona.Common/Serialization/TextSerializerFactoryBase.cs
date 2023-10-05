#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Common.Serialization
{
    public abstract class TextSerializerFactoryBase<TSerializer>
        : TextSerializerFactoryBase<TSerializer, ITextDeserializer>
        where TSerializer : ITextSerializer
    {
        public override sealed ITextDeserializer GetDeserializer(ISerializationContextProvider contextProvider)
        {
            throw new NotSupportedException("Deserialization not supported for format.");
        }
    }

    public abstract class TextSerializerFactoryBase<TSerializer, TDeserializer> : ITextSerializerFactory
        where TSerializer : ITextSerializer
        where TDeserializer : ITextDeserializer
    {
        public abstract TDeserializer GetDeserializer(ISerializationContextProvider contextProvider);
        public abstract TSerializer GetSerializer(ISerializationContextProvider contextProvider);


        ITextDeserializer ITextSerializerFactory.GetDeserializer(ISerializationContextProvider contextProvider)
        {
            return GetDeserializer(contextProvider);
        }


        ITextSerializer ITextSerializerFactory.GetSerializer(ISerializationContextProvider contextProvider)
        {
            return GetSerializer(contextProvider);
        }
    }
}

