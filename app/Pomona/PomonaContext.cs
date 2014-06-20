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

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaContext : IPomonaContext
    {
        private readonly ITextSerializerFactory serializerFactory;
        private NancyContext nancyContext;


        public PomonaContext(NancyContext nancyContext, ITextSerializerFactory serializerFactory)
        {
            if (nancyContext == null)
                throw new ArgumentNullException("nancyContext");
            if (serializerFactory == null)
                throw new ArgumentNullException("serializerFactory");
            this.nancyContext = nancyContext;
            this.serializerFactory = serializerFactory;
        }


        public NancyContext NancyContext
        {
            get { return this.nancyContext; }
        }


        public PomonaRequest CreateNestedRequest(PathNode node, HttpMethod httpMethod)
        {
            this.nancyContext = this.nancyContext ?? NancyContext;
            return new PomonaRequest(node, this, httpMethod);
        }


        public PomonaRequest CreateOuterRequest(PathNode pathNode)
        {
            var nancyRequest = NancyContext.Request;
            var method = (HttpMethod)Enum.Parse(typeof(HttpMethod), nancyRequest.Method, true);

            return new PomonaRequest(pathNode,
                this,
                method,
                nancyRequest.Body,
                GetExpandedPathsFromRequest(nancyRequest),
                nancyRequest.Headers,
                executeQueryable : true,
                hasQuery : true);
        }


        public T GetContext<T>()
        {
            if (typeof(T) == typeof(IPomonaContext) || typeof(T) == typeof(PomonaContext)
                || typeof(T) == typeof(IContextResolver))
                return (T)((object)this);

            return (T)this.nancyContext.Resolve(typeof(T));
        }


        public ITextDeserializer GetDeserializer()
        {
            return this.serializerFactory.GetDeserializer(NancyContext.GetSerializationContextProvider());
        }


        public object Resolve(Type type)
        {
            return NancyContext.Resolve(type);
        }


        private static string GetExpandedPathsFromRequest(Request nancyRequest)
        {
            var expansions = nancyRequest.Headers["X-Pomona-Expand"];
            if (nancyRequest.Query["$expand"].HasValue)
                expansions = expansions.Concat((string)nancyRequest.Query["$expand"]);
            var expandedPathsTemp = string.Join(",", expansions);
            return expandedPathsTemp;
        }
    }
}