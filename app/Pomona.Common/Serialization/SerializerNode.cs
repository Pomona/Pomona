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

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public abstract class SerializerNode : ISerializerNode
    {
        private readonly ISerializationContext context;
        private readonly string expandPath;
        private readonly TypeSpec expectedBaseType;
        private readonly bool isRemoved;
        private readonly ISerializerNode parentNode;
        private TypeSpec valueType;

        public bool IsRemoved
        {
            get { return this.isRemoved; }
        }

        #region Implementation of ISerializerNode

        protected SerializerNode(TypeSpec expectedBaseType,
                                 string expandPath,
                                 ISerializationContext context,
                                 ISerializerNode parentNode,
                                 bool isRemoved = false)
        {
            this.expectedBaseType = expectedBaseType;
            this.expandPath = expandPath;
            this.context = context;
            this.parentNode = parentNode;
            this.isRemoved = isRemoved;
        }


        public ISerializationContext Context
        {
            get { return this.context; }
        }

        public string ExpandPath
        {
            get { return this.expandPath; }
        }

        public virtual TypeSpec ExpectedBaseType
        {
            get { return this.expectedBaseType; }
        }

        public bool SerializeAsReference { get; set; }

        public virtual string Uri
        {
            get { return Context.GetUri(Value); }
        }

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

        public ISerializerNode ParentNode
        {
            get { return this.parentNode; }
        }

        #endregion
    }
}