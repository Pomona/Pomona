#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
    public class PropertyValueSerializerNode : ISerializerNode
    {
        private readonly ISerializationContext context;
        private string expandPath;
        private ISerializerNode parentNode;
        private IPropertyInfo property;
        private bool propertyIsLoaded;
        private object propertyValue;
        private IMappedType propertyValueType;

        #region Implementation of ISerializerNode

        public PropertyValueSerializerNode(
            ISerializerNode parentNode, IPropertyInfo property)
        {
            if (parentNode == null)
                throw new ArgumentNullException("parentNode");
            if (property == null)
                throw new ArgumentNullException("propertyMapping");
            this.parentNode = parentNode;
            this.property = property;
            context = parentNode.Context;
        }


        public string ExpandPath
        {
            get
            {
                if (expandPath == null)
                {
                    if (string.IsNullOrEmpty(parentNode.ExpandPath))
                        return property.LowerCaseName;

                    expandPath = string.Concat(parentNode.ExpandPath, ".", property.LowerCaseName);
                }
                return expandPath;
            }
        }

        public IMappedType ExpectedBaseType
        {
            get { return property.PropertyType; }
        }

        public ISerializationContext Context
        {
            get { return context; }
        }

        public bool SerializeAsReference { get; set; }

        public string Uri
        {
            get
            {
                if (property.PropertyType.SerializationMode == TypeSerializationMode.Complex)
                    return Context.GetUri(Value);
                return Context.GetUri(property, parentNode.Value);
            }
        }

        public object Value
        {
            get
            {
                if (!propertyIsLoaded)
                {
                    propertyValue = property.Getter(parentNode.Value);
                    propertyIsLoaded = true;
                }
                return propertyValue;
            }
        }

        public IMappedType ValueType
        {
            get
            {
                if (propertyValueType == null)
                {
                    propertyValueType = Value != null
                                            ? Context.GetClassMapping(Value.GetType())
                                            : ExpectedBaseType;
                }
                return propertyValueType;
            }
        }

        #endregion
    }
}