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

using System;
using Newtonsoft.Json.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ItemValueDeserializerNode : IDeserializerNode
    {
        private readonly IDeserializationContext context;
        private readonly IMappedType expectedBaseType;
        private object value;
        public IMappedType valueType;

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