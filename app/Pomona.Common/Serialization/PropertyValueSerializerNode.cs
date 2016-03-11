#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class PropertyValueSerializerNode : SerializerNode
    {
        private readonly PropertySpec property;
        private bool propertyIsLoaded;
        private object value;

        public PropertySpec Property
        {
            get { return this.property; }
        }

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
            this.property = property;
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


        public override TypeSpec ExpectedBaseType
        {
            get { return this.property.PropertyType; }
        }

        public override string Uri
        {
            get
            {
                if (this.ValueType.SerializationMode == TypeSerializationMode.Structured)
                    return base.Uri;

                return Context.GetUri(this.property, ParentNode.Value);
            }
        }

        public override object Value
        {
            get
            {
                if (!this.propertyIsLoaded)
                {
                    this.value = this.property.GetValue(ParentNode.Value, Context);
                    this.propertyIsLoaded = true;
                }
                return this.value;
            }
        }

        #endregion
    }
}