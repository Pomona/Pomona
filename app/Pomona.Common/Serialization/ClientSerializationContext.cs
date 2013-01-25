using System;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientSerializationContext : ISerializationContext
    {
        private readonly ITypeMapper typeMapper;


        public ClientSerializationContext(ITypeMapper typeMapper)
        {
            this.typeMapper = typeMapper;
        }

        #region Implementation of ISerializationContext

        public IMappedType GetClassMapping(Type type)
        {
            return typeMapper.GetClassMapping(type);
        }


        public string GetUri(object value)
        {
            var hasUriResource = value as IHasResourceUri;
            return hasUriResource != null ? hasUriResource.Uri : null;
        }


        public string GetUri(IPropertyInfo property, object value)
        {
            return "http://todo";
        }


        public bool PathToBeExpanded(string expandPath)
        {
            return true;
        }


        public void Serialize<TWriter>(ISerializerNode node, ISerializer<TWriter> serializer, TWriter writer)
            where TWriter : ISerializerWriter
        {
            if (node.Value is IClientResource && !(node.Value is PutResourceBase))
            {
                node.SerializeAsReference = true;
            }
            serializer.SerializeNode(node, writer);
        }

        #endregion
    }
}