#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.Loading;
using Pomona.Common.Serialization;

namespace Pomona.Common.Internals
{
    internal class ClientSerializationContextProvider : ISerializationContextProvider
    {
        private readonly IPomonaClient client;
        private readonly ClientTypeMapper clientTypeMapper;
        private readonly IResourceLoader resourceLoader;


        internal ClientSerializationContextProvider(ClientTypeMapper clientTypeMapper, IPomonaClient client, IResourceLoader resourceLoader)
        {
            if (clientTypeMapper == null)
                throw new ArgumentNullException(nameof(clientTypeMapper));

            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (resourceLoader == null)
                throw new ArgumentNullException(nameof(resourceLoader));

            this.clientTypeMapper = clientTypeMapper;
            this.client = client;
            this.resourceLoader = resourceLoader;
        }


        public IDeserializationContext GetDeserializationContext(DeserializeOptions options)
        {
            return new ClientDeserializationContext(this.clientTypeMapper, this.client, this.resourceLoader);
        }


        public ISerializationContext GetSerializationContext(SerializeOptions options)
        {
            return new ClientSerializationContext(this.clientTypeMapper);
        }
    }
}

