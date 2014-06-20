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
using System.Collections.Generic;
using System.IO;

using Nancy;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

namespace Pomona
{
    public class PomonaRequest
    {
        private readonly Stream body;
        private readonly IPomonaContext context;
        private readonly string expandedPaths;

        private readonly HttpMethod method;
        private readonly PathNode node;
        private object deserializedBody;
        private RequestHeaders headers;
        private readonly bool executeQueryable;
        private readonly bool hasQuery;

        public bool HasQuery
        {
            get { return this.hasQuery; }
        }

        public bool ExecuteQueryable
        {
            get { return this.executeQueryable; }
        }


        public PomonaRequest(PathNode node,
            IPomonaContext context,
            HttpMethod method,
            Stream body = null,
            string expandedPaths = null,
            RequestHeaders headers = null,
            bool executeQueryable = false,
            bool hasQuery = false)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (context == null)
                throw new ArgumentNullException("context");
            this.node = node;
            this.context = context;
            this.body = body;
            this.expandedPaths = expandedPaths;
            this.method = method;
            this.headers = headers;
            this.executeQueryable = executeQueryable;
            this.hasQuery = hasQuery;
        }


        public string ExpandedPaths
        {
            get { return this.expandedPaths; }
        }

        public RequestHeaders Headers
        {
            get
            {
                return this.headers ?? (this.headers = new RequestHeaders(new Dictionary<string, IEnumerable<string>>()));
            }
        }

        public HttpMethod Method
        {
            get { return this.method; }
        }

        public PathNode Node
        {
            get { return this.node; }
        }

        public TypeMapper TypeMapper
        {
            get { return (TypeMapper)this.node.TypeMapper; }
        }

        internal IPomonaContext Context
        {
            get { return this.context; }
        }


        public object Bind(TypeSpec type = null, object patchedObject = null)
        {
            if (this.deserializedBody == null)
            {
                if (this.body == null)
                    throw new InvalidOperationException("No http body to deserialize.");
                if (Method == HttpMethod.Post)
                    type = type ?? Node.ExpectedPostType;

                if (Method == HttpMethod.Patch)
                {
                    patchedObject = patchedObject ?? Node.Value;
                    if (patchedObject != null)
                        type = TypeMapper.GetClassMapping(patchedObject.GetType());
                }

                this.deserializedBody = Deserialize(type as TransformedType, this.body, patchedObject);
            }
            return this.deserializedBody;
        }


        public PomonaQuery ParseQuery()
        {
            if (!HasQuery)
                throw new InvalidOperationException("Unable to parse non-existing query.");

            var collectionNode = this.node as ResourceCollectionNode;
            if (collectionNode == null)
                throw new InvalidOperationException("Queries are only valid for Queryable nodes.");

            return
                new PomonaHttpQueryTransformer(this.node.TypeMapper,
                    new QueryExpressionParser(new QueryTypeResolver(this.node.TypeMapper))).TransformRequest(
                        this.context.NancyContext,
                        collectionNode.ItemResourceType);
        }


        private object Deserialize(TransformedType expectedBaseType, Stream body, object patchedObject = null)
        {
            using (var textReader = new StreamReader(body))
            {
                return this.context.GetDeserializer().Deserialize(textReader,
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