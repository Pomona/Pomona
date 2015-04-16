#region License
// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using Nancy;

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ServerSerializationContextProvider : ISerializationContextProvider
    {
        private readonly TypeMapper typeMapper;
        private readonly IUriResolver uriResolver;
        private readonly IResourceResolver resourceResolver;
        private readonly IContainer container;


        public ServerSerializationContextProvider(TypeMapper typeMapper, IUriResolver uriResolver, IResourceResolver resourceResolver, IContainer container)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (uriResolver == null)
                throw new ArgumentNullException("uriResolver");
            if (resourceResolver == null)
                throw new ArgumentNullException("resourceResolver");
            if (container == null)
                throw new ArgumentNullException("container");
            this.typeMapper = typeMapper;
            this.uriResolver = uriResolver;
            this.resourceResolver = resourceResolver;
            this.container = container;
        }


        public IDeserializationContext GetDeserializationContext(DeserializeOptions options)
        {
            options = options ?? new DeserializeOptions();
            return new ServerDeserializationContext(typeMapper, resourceResolver, options.TargetNode, container);
        }


        public ISerializationContext GetSerializationContext(SerializeOptions options)
        {
            options = options ?? new SerializeOptions();
            return new ServerSerializationContext(typeMapper, options.ExpandedPaths, false, uriResolver, container);
        }
    }
}