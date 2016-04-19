#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public abstract class SerializerNode : ISerializerNode
    {
        private TypeSpec valueType;

        public bool IsRemoved { get; }

        #region Implementation of ISerializerNode

        protected SerializerNode(TypeSpec expectedBaseType,
                                 string expandPath,
                                 ISerializationContext context,
                                 ISerializerNode parentNode,
                                 bool isRemoved = false)
        {
            ExpectedBaseType = expectedBaseType;
            ExpandPath = expandPath;
            Context = context;
            ParentNode = parentNode;
            IsRemoved = isRemoved;
        }


        public ISerializationContext Context { get; }

        public string ExpandPath { get; }

        public virtual TypeSpec ExpectedBaseType { get; }

        public bool SerializeAsReference { get; set; }

        public virtual string Uri => Context.GetUri(Value);

        public abstract object Value { get; }

        public TypeSpec ValueType
        {
            get
            {
                if (this.valueType == null)
                {
                    this.valueType = Value != null
                        ? Context.GetClassMapping(Value.GetType())
                        : ExpectedBaseType;
                }
                return this.valueType;
            }
        }

        public ISerializerNode ParentNode { get; }

        #endregion
    }
}