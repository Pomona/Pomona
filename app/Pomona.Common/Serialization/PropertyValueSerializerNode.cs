#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class PropertyValueSerializerNode : SerializerNode
    {
        private bool propertyIsLoaded;
        private object value;

        public PropertySpec Property { get; }

        #region Implementation of ISerializerNode

        public PropertyValueSerializerNode(
            ISerializerNode parentNode,
            PropertySpec property)
            : base(
                property != null ? property.PropertyType : null, GetExpandPath(parentNode, property),
                parentNode != null ? parentNode.Context : null, parentNode)
        {
            if (parentNode == null)
                throw new ArgumentNullException(nameof(parentNode));
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            Property = property;
        }


        private static string GetExpandPath(ISerializerNode parentNode, PropertySpec property)
        {
            if (string.IsNullOrEmpty(parentNode.ExpandPath))
            {
                if (property is QueryResultType.ItemsPropertySpec)
                    return string.Empty;
                return property.LowerCaseName;
            }

            return string.Concat(parentNode.ExpandPath, ".", property.LowerCaseName);
        }


        public override TypeSpec ExpectedBaseType => Property.PropertyType;

        public override string Uri
        {
            get
            {
                if (ValueType.SerializationMode == TypeSerializationMode.Structured)
                    return base.Uri;

                return Context.GetUri(Property, ParentNode.Value);
            }
        }

        public override object Value
        {
            get
            {
                if (!this.propertyIsLoaded)
                {
                    this.value = Property.GetValue(ParentNode.Value, Context);
                    this.propertyIsLoaded = true;
                }
                return this.value;
            }
        }

        #endregion
    }
}
