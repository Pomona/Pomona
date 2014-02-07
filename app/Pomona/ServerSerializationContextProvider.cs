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

namespace Pomona
{
    public class ServerSerializationContextProvider : ISerializationContextProvider
    {
        private readonly IUriResolver uriResolver;
        private readonly IResourceResolver resourceResolver;
        private readonly NancyContext nancyContext;


        public ServerSerializationContextProvider(IUriResolver uriResolver, IResourceResolver resourceResolver, NancyContext nancyContext)
        {
            if (uriResolver == null)
                throw new ArgumentNullException("uriResolver");
            if (resourceResolver == null)
                throw new ArgumentNullException("resourceResolver");
            if (nancyContext == null)
                throw new ArgumentNullException("nancyContext");
            this.uriResolver = uriResolver;
            this.resourceResolver = resourceResolver;
            this.nancyContext = nancyContext;
        }


        public IDeserializationContext GetDeserializationContext(DeserializeOptions options)
        {
            options = options ?? new DeserializeOptions();
            return new ServerDeserializationContext(uriResolver.TypeMapper, resourceResolver, options.TargetNode, nancyContext);
        }


        public ISerializationContext GetSerializationContext(SerializeOptions options)
        {
            options = options ?? new SerializeOptions();
            return new ServerSerializationContext(options.ExpandedPaths, false, uriResolver);
        }
    }
}