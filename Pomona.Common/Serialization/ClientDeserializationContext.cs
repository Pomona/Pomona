using System;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientDeserializationContext : IDeserializationContext
    {
        private readonly ClientBase client;
        private ITypeMapper typeMapper;

        [Obsolete("Solely here for testing purposes")]
        public ClientDeserializationContext(ITypeMapper typeMapper) : this(typeMapper, null)
        {
        }


        public ClientDeserializationContext(ITypeMapper typeMapper, ClientBase client)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
            this.client = client;
        }

        #region Implementation of IDeserializationContext

        public void Deserialize<TReader>(IDeserializerNode node, IDeserializer<TReader> deserializer, TReader reader)
            where TReader : ISerializerReader
        {
            deserializer.DeserializeNode(node, reader);
            if (node.Uri != null)
            {
                var uriResource = node.Value as IHasResourceUri;
                if (uriResource != null)
                {
                    uriResource.Uri = node.Uri;
                }
            }
        }


        public IMappedType GetTypeByName(string typeName)
        {
            return typeMapper.GetClassMapping(typeName);
        }


        public object CreateReference(IMappedType type, string uri)
        {
            if (type.SerializationMode == TypeSerializationMode.Array)
            {
                var lazyListType = typeof (LazyListProxy<>).MakeGenericType(type.ElementType.MappedTypeInstance);
                return Activator.CreateInstance(lazyListType, uri, client);
            }
            if (type is ClientType && type.SerializationMode == TypeSerializationMode.Complex)
            {
                var clientType = (ClientType) type;
                var proxyType = clientType.ResourceInfo.LazyProxyType;
                var refobj = (LazyProxyBase) Activator.CreateInstance(proxyType);
                refobj.Client = client;
                refobj.ProxyTargetType = clientType.ResourceInfo.PocoType;
                ((IHasResourceUri) refobj).Uri = uri;
                return refobj;
            }
            if (type.MappedType == typeof (ClientRepository<,>))
            {
                return Activator.CreateInstance(type.MappedTypeInstance, client, uri);
            }
            throw new NotImplementedException("Don't know how to make reference to type " + type.Name + " yet!");
        }

        #endregion
    }
}