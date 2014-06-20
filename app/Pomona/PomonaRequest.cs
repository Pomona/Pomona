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
using System.IO;

using Nancy;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaRequest
    {
        private readonly NancyContext context;

        private readonly HttpMethod method;
        private readonly PathNode node;
        private readonly ITextSerializerFactory serializerFactory;
        private object deserializedBody;


        public PomonaRequest(PathNode node,
            NancyContext context,
            ITextSerializerFactory serializerFactory)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (context == null)
                throw new ArgumentNullException("context");
            if (serializerFactory == null)
                throw new ArgumentNullException("serializerFactory");
            this.node = node;
            this.context = context;
            this.serializerFactory = serializerFactory;
            this.method = (HttpMethod)Enum.Parse(typeof(HttpMethod), context.Request.Method, true);
        }


        public string ExpandedPaths
        {
            get
            {
                var expansions = NancyRequest.Headers["X-Pomona-Expand"];
                if (NancyRequest.Query["$expand"].HasValue)
                    expansions = expansions.Concat((string)NancyRequest.Query["$expand"]);
                return string.Join(",", expansions);
            }
        }

        public RequestHeaders Headers
        {
            get { return NancyRequest.Headers; }
        }

        public HttpMethod Method
        {
            get { return this.method; }
        }

        public NancyContext NancyContext
        {
            get { return this.context; }
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


        public object Bind(TypeSpec type = null, object patchedObject = null)
        {
            if (this.deserializedBody == null)
            {
                if (Method == HttpMethod.Post)
                    type = type ?? Node.ExpectedPostType;

                if (Method == HttpMethod.Patch)
                {
                    patchedObject = patchedObject ?? Node.Value;
                    if (patchedObject != null)
                        type = TypeMapper.GetClassMapping(patchedObject.GetType());
                }

                this.deserializedBody = Deserialize(type as TransformedType, NancyRequest.Body, patchedObject);
            }
            return this.deserializedBody;
        }


        public PomonaQuery ParseQuery()
        {
            var collectionNode = this.node as ResourceCollectionNode;
            if (collectionNode == null)
                throw new InvalidOperationException("Queries are only valid for Queryable nodes.");

            return
                new PomonaHttpQueryTransformer(this.node.TypeMapper,
                    new QueryExpressionParser(new QueryTypeResolver(this.node.TypeMapper))).TransformRequest(
                        this.context,
                        collectionNode.ItemResourceType);
        }


        private object Deserialize(TransformedType expectedBaseType, Stream body, object patchedObject = null)
        {
            using (var textReader = new StreamReader(body))
            {
                return this.serializerFactory.GetDeserializer(NancyContext.GetSerializationContextProvider()).Deserialize(textReader,
                    new DeserializeOptions()
                    {
                        Target = patchedObject,
                        ExpectedBaseType = expectedBaseType,
                        TargetNode = Node
                    });
            }
        }
    }
}