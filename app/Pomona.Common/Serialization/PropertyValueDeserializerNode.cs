#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class PropertyValueDeserializerNode : IDeserializerNode
    {
        private readonly IDeserializationContext context;
        private readonly IDeserializerNode parent;
        private readonly IPropertyInfo property;

        private string expandPath;
        private IMappedType valueType;


        public PropertyValueDeserializerNode(IDeserializerNode parent, IPropertyInfo property)
        {
            this.parent = parent;
            this.property = property;
            this.valueType = property.PropertyType;
            this.context = parent.Context;
        }

        #region Implementation of IDeserializerNode

        public IDeserializationContext Context
        {
            get { return this.context; }
        }

        public string ExpandPath
        {
            get
            {
                if (this.expandPath == null)
                {
                    if (string.IsNullOrEmpty(this.parent.ExpandPath))
                        return this.property.LowerCaseName;

                    this.expandPath = string.Concat(this.parent.ExpandPath, ".", this.property.LowerCaseName);
                }
                return this.expandPath;
            }
        }

        public IMappedType ExpectedBaseType
        {
            get { return this.property.PropertyType; }
        }

        public DeserializerNodeOperation Operation { get; set; }

        public IDeserializerNode Parent
        {
            get { return this.parent; }
        }

        public string Uri { get; set; }
        public object Value { get; set; }

        public IMappedType ValueType
        {
            get { return this.valueType; }
        }


        public void CheckItemAccessRights(HttpMethod method)
        {
            Context.CheckPropertyItemAccessRights(this.property, method);
        }


        public void SetProperty(IPropertyInfo property, object propertyValue)
        {
            this.context.SetProperty(this, property, propertyValue);
        }


        public void SetValueType(string typeName)
        {
            this.valueType = Context.GetTypeByName(typeName);
        }

        #endregion

        public IPropertyInfo Property
        {
            get { return this.property; }
        }
    }
}