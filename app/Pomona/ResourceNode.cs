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

using System;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;

namespace Pomona
{
    public class ResourceNode : PathNode
    {
        private readonly ResourceType expectedType;
        private readonly System.Lazy<ResourceType> type;
        private readonly System.Lazy<object> value;


        public ResourceNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            Func<object> valueFetcher,
            ResourceType expectedType)
            : base(typeMapper, parent, name, PathNodeType.Resource)
        {
            this.value = new System.Lazy<object>(valueFetcher);
            this.expectedType = expectedType;

            this.type = new System.Lazy<ResourceType>(() =>
            {
                if (expectedType != null && !expectedType.SubTypes.Any())
                    return expectedType;
                var localValue = Value;
                if (Value == null)
                    return expectedType;
                return (ResourceType)typeMapper.GetClassMapping(localValue.GetType());
            });
        }


        public override HttpMethod AllowedMethods
        {
            get
            {
                // TODO: Currently there's no good way to define that POST to a resource is allowed while POST to collection is not allowed.
                // The code below is a workaround: if there's any defined PostHandlers we will allow POST.
                return HttpMethod.Delete | HttpMethod.Get | HttpMethod.Patch | HttpMethod.Post | HttpMethod.Put;
                //return Type.AllowedMethods | (Type.PostHandlers.Any() ? HttpMethod.Post : 0);
            }
        }

        public override bool Exists
        {
            get { return Value != null; }
        }

        public override bool IsLoaded
        {
            get { return this.value.IsValueCreated; }
        }

        public new ResourceType Type
        {
            get { return this.type.Value; }
        }

        public override object Value
        {
            get { return this.value.Value; }
        }


        public override PathNode GetChildNode(string name)
        {
            PropertySpec property;
            if (
                !(this.expectedType.TryGetPropertyByUriName(name, out property)
                  || Type.TryGetPropertyByUriName(name, out property)))
                throw new ResourceNotFoundException("Resource not found");

            return CreateNode(TypeMapper, this, name, () => property.GetValue(Value), property.PropertyType);
        }


        protected override TypeSpec OnGetType()
        {
            return Type;
        }


        protected override IPomonaRequestProcessor OnGetRequestProcessor(PomonaRequest request)
        {
            return Type.ResourceHandlers.EmptyIfNull().Select(HandlerRequestProcessor.Create).FirstOrDefault();
        }
    }
}