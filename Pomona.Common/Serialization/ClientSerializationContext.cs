using System;

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
            return "http://todo";
        }


        public string GetUri(IPropertyInfo property, object value)
        {
            return "http://todo";
        }


        public bool PathToBeExpanded(string expandPath)
        {
            return true;
        }

        #endregion
    }
}