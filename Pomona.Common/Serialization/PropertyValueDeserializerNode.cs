using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class PropertyValueDeserializerNode : IDeserializerNode
    {
        private IDeserializationContext context;
        private IDeserializerNode parent;
        private IPropertyInfo property;
        private IMappedType valueType;


        public PropertyValueDeserializerNode(IDeserializerNode parent, IPropertyInfo property)
        {
            this.parent = parent;
            this.property = property;
            valueType = property.PropertyType;
            context = parent.Context;
        }

        #region Implementation of IDeserializerNode

        public IDeserializationContext Context
        {
            get { return context; }
        }

        public IMappedType ExpectedBaseType
        {
            get { return property.PropertyType; }
        }

        public object Value { get; set; }

        public string Uri { get; set; }

        public void SetValueType(string typeName)
        {
            valueType = Context.GetTypeByName(typeName);
        }


        public IMappedType ValueType
        {
            get { return valueType; }
        }

        #endregion
    }
}