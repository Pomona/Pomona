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

using System.Linq;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ResourceNode : PathNode
    {
        private readonly ResourceType type;
        private readonly object value;


        public ResourceNode(ITypeMapper typeMapper, PathNode parent, string name, object value, ResourceType type)
            : base(typeMapper, parent, name)
        {
            this.value = value;
            this.type = type;
        }


        public override HttpMethod AllowedMethods
        {
            get
            {
                // TODO: Currently there's no good way to define that POST to a resource is allowed while POST to collection is not allowed.
                // The code below is a workaround: if there's any defined PostHandlers we will allow POST.
                return this.type.AllowedMethods | (type.PostHandlers.Any() ? HttpMethod.Post : 0);
            }
        }

        public new ResourceType Type
        {
            get { return this.type; }
        }

        public override object Value
        {
            get { return this.value; }
        }


        public override PathNode GetChildNode(string name)
        {
            IPropertyInfo property;
            if (!Type.TryGetPropertyByUriName(name, out property))
                throw new ResourceNotFoundException("Resource not found");

            var value = property.Getter(Value);
            return CreateNode(TypeMapper, this, name, value, property.PropertyType);
        }


        protected override IMappedType OnGetType()
        {
            return this.type;
        }
    }
}