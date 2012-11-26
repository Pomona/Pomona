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

namespace Pomona.Serialization
{
    public class ItemValueSerializerNode<TMappedType> : ISerializerNode<TMappedType>
        where TMappedType : class, IMappedType
    {
        private string expandPath;
        private TMappedType expectedBaseType;
        private FetchContext fetchContext;
        private object value;
        private TMappedType valueType;

        #region Implementation of ISerializerNode

        public ItemValueSerializerNode(
            object value, TMappedType expectedBaseType, string expandPath, FetchContext fetchContext)
        {
            this.value = value;
            this.expectedBaseType = expectedBaseType;
            this.expandPath = expandPath;
            this.fetchContext = fetchContext;
        }


        public string ExpandPath
        {
            get { return this.expandPath; }
        }

        public TMappedType ExpectedBaseType
        {
            get { return this.expectedBaseType; }
        }

        public FetchContext FetchContext
        {
            get { return this.fetchContext; }
        }

        public bool SerializeAsReference
        {
            get { return !(this.expectedBaseType.IsAlwaysExpanded || this.fetchContext.PathToBeExpanded(this.expandPath)); }
        }

        public string Uri
        {
            get { return FetchContext.Session.GetUri(Value); }
        }

        public object Value
        {
            get { return this.value; }
        }

        public TMappedType ValueType
        {
            get
            {
                if (this.valueType == null)
                    this.valueType = (TMappedType)this.fetchContext.TypeMapper.GetClassMapping(Value.GetType());
                return this.valueType;
            }
        }

        IMappedType ISerializerNode.ExpectedBaseType
        {
            get { return ExpectedBaseType; }
        }

        IMappedType ISerializerNode.ValueType
        {
            get { return ValueType; }
        }

        #endregion
    }

    public static class ItemValueSerializerNode
    {
        public static ISerializerNode Create(
            object value, IMappedType expectedBaseType, string expandPath, FetchContext fetchContext)
        {
            if (expectedBaseType is SharedType)
                return new ItemValueSerializerNode<SharedType>(
                    value, (SharedType)expectedBaseType, expandPath, fetchContext);
            if (expectedBaseType is TransformedType)
                return new ItemValueSerializerNode<TransformedType>(
                    value, (TransformedType)expectedBaseType, expandPath, fetchContext);
            if (expectedBaseType is EnumType)
                return new ItemValueSerializerNode<EnumType>(
                    value, (EnumType)expectedBaseType, expandPath, fetchContext);

            throw new InvalidOperationException("Mapped type not recognized.");
        }
    }
}