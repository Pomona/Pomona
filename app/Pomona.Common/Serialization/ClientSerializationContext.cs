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

using Pomona.Common.Proxies;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientSerializationContext : ISerializationContext
    {
        private readonly ITypeResolver typeMapper;


        public ClientSerializationContext(ITypeResolver typeMapper)
        {
            this.typeMapper = typeMapper;
        }


        public T GetInstance<T>()
        {
            return new NoContainer().GetInstance<T>();
        }

        #region Implementation of ISerializationContext

        public TypeSpec GetClassMapping(Type type)
        {
            return this.typeMapper.FromType(type);
        }


        public string GetUri(object value)
        {
            var hasUriResource = value as IHasResourceUri;
            return hasUriResource != null ? hasUriResource.Uri : null;
        }


        public string GetUri(PropertySpec property, object value)
        {
            return "http://todo";
        }


        public bool PathToBeExpanded(string expandPath)
        {
            return true;
        }


        public void Serialize(ISerializerNode node, Action<ISerializerNode> serializeNodeAction)
        {
            if (node.Value is IClientResource &&
                !(node.Value is PostResourceBase) &&
                !(node.Value is IDelta) &&
                !node.IsRemoved)
                node.SerializeAsReference = true;

            serializeNodeAction(node);
        }

        #endregion
    }
}