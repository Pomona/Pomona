#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.Proxies;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientSerializationContext : ISerializationContext
    {
        private readonly ITypeResolver typeMapper;


        public ClientSerializationContext(ITypeResolver typeMapper)
        {
            this.typeMapper = typeMapper;
        }


        public T GetInstance<T>()
        {
            return new NoContainer().GetInstance<T>();
        }

        #region Implementation of ISerializationContext

        public TypeSpec GetClassMapping(Type type)
        {
            return this.typeMapper.FromType(type);
        }


        public string GetUri(object value)
        {
            var hasUriResource = value as IHasResourceUri;
            return hasUriResource != null ? hasUriResource.Uri : null;
        }


        public string GetUri(PropertySpec property, object value)
        {
            return "http://todo";
        }


        public bool PathToBeExpanded(string expandPath)
        {
            return true;
        }


        public void Serialize(ISerializerNode node, Action<ISerializerNode> serializeNodeAction)
        {
            if (node.Value is IClientResource &&
                !(node.Value is PostResourceBase) &&
                !(node.Value is IDelta) &&
                !node.IsRemoved)
                node.SerializeAsReference = true;

            serializeNodeAction(node);
        }

        #endregion
    }
}
