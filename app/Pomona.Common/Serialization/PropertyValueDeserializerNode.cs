#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class PropertyValueDeserializerNode : IDeserializerNode
    {
        private string expandPath;
        private object value;


        public PropertyValueDeserializerNode(IDeserializerNode parent, PropertySpec property)
        {
            Parent = parent;
            Property = property;
            ValueType = property.PropertyType;
            Context = parent.Context;
        }


        public PropertySpec Property { get; }

        #region Implementation of IDeserializerNode

        public IDeserializationContext Context { get; }

        public string ExpandPath
        {
            get
            {
                if (this.expandPath == null)
                {
                    if (string.IsNullOrEmpty(Parent.ExpandPath))
                        return Property.LowerCaseName;

                    this.expandPath = string.Concat(Parent.ExpandPath, ".", Property.LowerCaseName);
                }
                return this.expandPath;
            }
        }

        public TypeSpec ExpectedBaseType => Property.PropertyType;

        public DeserializerNodeOperation Operation { get; set; }

        public IDeserializerNode Parent { get; }

        public string Uri { get; set; }

        IResourceNode IResourceNode.Parent => Parent ?? Context.TargetNode;

        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        TypeSpec IResourceNode.ResultType => ExpectedBaseType;

        public TypeSpec ValueType { get; private set; }


        public void CheckItemAccessRights(HttpMethod method)
        {
            Context.CheckPropertyItemAccessRights(Property, method);
        }


        public void SetProperty(PropertySpec property, object propertyValue)
        {
            Context.SetProperty(this, property, propertyValue);
        }


        public void SetValueType(string typeName)
        {
            ValueType = Context.GetTypeByName(typeName);
        }


        public void SetValueType(Type type)
        {
            ValueType = Context.GetClassMapping(type);
        }


        public void CheckAccessRights(HttpMethod method)
        {
            Context.CheckAccessRights(Property, method);
        }

        #endregion
    }
}