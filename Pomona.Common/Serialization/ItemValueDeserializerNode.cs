using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ItemValueDeserializerNode : IDeserializerNode
    {
        private IDeserializationContext context;
        private IMappedType expectedBaseType;
        public IMappedType valueType;

        #region Implementation of IDeserializerNode

        public ItemValueDeserializerNode(IMappedType expectedBaseType, IDeserializationContext context)
        {
            this.expectedBaseType = expectedBaseType;
            this.context = context;
            valueType = expectedBaseType;
        }


        public IDeserializationContext Context
        {
            get { return context; }
        }

        public IMappedType ExpectedBaseType
        {
            get { return expectedBaseType; }
        }

        public string Uri { get; set; }

        public object Value { get; set; }

        public IMappedType ValueType
        {
            get { return valueType; }
        }


        public void SetValueType(string typeName)
        {
            valueType = context.GetTypeByName(typeName);
        }

        #endregion
    }
}