#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ItemValueDeserializerNode : IDeserializerNode
    {
        public TypeSpec valueType;

        #region Implementation of IDeserializerNode

        public ItemValueDeserializerNode(TypeSpec expectedBaseType,
                                         IDeserializationContext context,
                                         string expandPath = "",
                                         IDeserializerNode parent = null)
        {
            Parent = parent;
            ExpectedBaseType = expectedBaseType;
            Context = context;
            ExpandPath = expandPath;
            this.valueType = expectedBaseType;
        }


        public IDeserializationContext Context { get; }

        public TypeSpec ExpectedBaseType { get; }

        public string ExpandPath { get; }

        public string Uri { get; set; }

        IResourceNode IResourceNode.Parent
        {
            get { return Parent ?? Context.TargetNode; }
        }

        public object Value { get; set; }

        TypeSpec IResourceNode.ResultType
        {
            get { return ExpectedBaseType; }
        }


        public void CheckItemAccessRights(HttpMethod method)
        {
        }


        public IDeserializerNode Parent { get; }

        public TypeSpec ValueType
        {
            get { return this.valueType; }
        }

        public DeserializerNodeOperation Operation { get; set; }


        public void SetValueType(string typeName)
        {
            this.valueType = Context.GetTypeByName(typeName);
        }


        public void SetValueType(Type type)
        {
            this.valueType = Context.GetClassMapping(type);
        }


        public void CheckAccessRights(HttpMethod method)
        {
        }


        public void SetProperty(PropertySpec property, object propertyValue)
        {
            Context.SetProperty(this, property, propertyValue);
        }

        #endregion
    }
}