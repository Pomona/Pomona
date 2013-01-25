using System;
using Newtonsoft.Json.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ItemValueDeserializerNode : IDeserializerNode
    {
        private IDeserializationContext context;
        private IMappedType expectedBaseType;
        public IMappedType valueType;
        private object value;

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

        public object Value
        {
            get { return value; }
            set
            {
                if (value is JToken)
                    throw new InvalidOperationException("Fuck you!");
                this.value = value;
            }
        }

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