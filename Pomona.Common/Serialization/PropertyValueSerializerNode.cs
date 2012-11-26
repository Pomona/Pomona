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
        private readonly ISerializationContext fetchContext;
        private string expandPath;
        private ISerializerNode parentNode;
        private bool propertyIsLoaded;
        private IPropertyInfo propertyMapping;
        private object propertyValue;
        private IMappedType propertyValueType;

        #region Implementation of ISerializerNode

        public PropertyValueSerializerNode(
            ISerializerNode parentNode, IPropertyInfo propertyMapping, ISerializationContext fetchContext)
        {
            if (parentNode == null)
                throw new ArgumentNullException("parentNode");
            if (propertyMapping == null)
                throw new ArgumentNullException("propertyMapping");
            if (fetchContext == null)
                throw new ArgumentNullException("fetchContext");
            this.parentNode = parentNode;
            this.propertyMapping = propertyMapping;
            this.fetchContext = fetchContext;
        }


        public string ExpandPath
        {
            get
            {
                if (this.expandPath == null)
                {
                    if (string.IsNullOrEmpty(this.parentNode.ExpandPath))
                        return this.propertyMapping.LowerCaseName;

                    this.expandPath = string.Concat(this.parentNode.ExpandPath, ".", this.propertyMapping.LowerCaseName);
                }
                return this.expandPath;
            }
        }

        public IMappedType ExpectedBaseType
        {
            get { return this.propertyMapping.PropertyType; }
        }

        public ISerializationContext FetchContext
        {
            get { return this.fetchContext; }
        }

        public bool SerializeAsReference
        {
            get
            {
                if (ExpectedBaseType.IsAlwaysExpanded)
                    return false;
                if (FetchContext.PathToBeExpanded(ExpandPath))
                    return false;
                if (ExpectedBaseType.IsCollection && FetchContext.PathToBeExpanded(ExpandPath + "!"))
                    return false;

                return true;
            }
        }

        public string Uri
        {
            get
            {
                if (this.propertyMapping.PropertyType.SerializationMode == TypeSerializationMode.Complex)
                    return FetchContext.GetUri(Value);
                return FetchContext.GetUri(this.propertyMapping, this.parentNode.Value);
            }
        }

        public object Value
        {
            get
            {
                if (!this.propertyIsLoaded)
                {
                    this.propertyValue = this.propertyMapping.Getter(this.parentNode.Value);
                    this.propertyIsLoaded = true;
                }
                return this.propertyValue;
            }
        }

        public IMappedType ValueType
        {
            get
            {
                if (this.propertyValueType == null)
                {
                    this.propertyValueType = Value != null
                                                 ? FetchContext.GetClassMapping(Value.GetType())
                                                 : ExpectedBaseType;
                }
                return this.propertyValueType;
            }
        }

        #endregion
    }
}