#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright � 2013 Karsten Nikolai Strand
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
using System.IO;

using Nancy;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaRequest
    {
        private readonly NancyContext context;
        private readonly PathNode node;
        private readonly IResourceResolver resourceResolver;


        public PomonaRequest(PathNode node, NancyContext context, IResourceResolver resourceResolver)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (context == null)
                throw new ArgumentNullException("context");
            if (resourceResolver == null)
                throw new ArgumentNullException("resourceResolver");
            this.node = node;
            this.context = context;
            this.resourceResolver = resourceResolver;
        }


        public string ExpandedPaths
        {
            get { return NancyRequest.Query["$expand"].HasValue ? NancyRequest.Query["$expand"] : string.Empty; }
        }

        public HttpMethod Method
        {
            get { return (HttpMethod)Enum.Parse(typeof(HttpMethod), NancyRequest.Method, true); }
        }

        public Request NancyRequest
        {
            get { return this.context.Request; }
        }

        public PathNode Node
        {
            get { return this.node; }
        }

        public TypeMapper TypeMapper
        {
            get { return (TypeMapper)this.node.TypeMapper; }
        }


        public object Bind()
        {
            if (Method != HttpMethod.Post)
                throw new NotImplementedException("Only knows how to deserialize without specifying type on Post.");
            return Bind(Node.ExpectedPostType);
        }


        public object Bind(IMappedType type)
        {
            // TODO: Refactor binding, currently hard-coded to JSON
            var transformedType = type as TransformedType;
            if (transformedType == null)
                throw new NotSupportedException("Only knows how to deserialize a TransformedType");
            return Deserialize(transformedType, NancyRequest.Body);
        }


        public PomonaQuery ParseQuery()
        {
            var queryableNode = this.node as QueryableNode;
            if (queryableNode == null)
                throw new InvalidOperationException("Queries are only valid for Queryable nodes.");

            return
                new PomonaHttpQueryTransformer(this.node.TypeMapper,
                    new QueryExpressionParser(new QueryTypeResolver(this.node.TypeMapper))).TransformRequest(
                        this.context,
                        queryableNode.ItemResourceType);
        }


        private object Deserialize(TransformedType expectedBaseType, Stream body, object patchedObject = null)
        {
            using (var textReader = new StreamReader(body))
            {
                var deserializationContext = new ServerDeserializationContext(TypeMapper, this.resourceResolver);
                return TypeMapper.SerializerFactory.GetDeserializer().Deserialize(textReader,
                    expectedBaseType,
                    deserializationContext,
                    patchedObject);
            }
        }
    }
}