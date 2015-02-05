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
using Pomona.Routing;

namespace Pomona
{
    public class PomonaContext
    {
        private readonly Type acceptType;
        private readonly bool executeQueryable;
        private readonly string expandedPaths;
        private object deserializedBody;


        public PomonaContext(UrlSegment node,
                             PomonaRequest request = null,
                             string expandedPaths = null,
                             bool executeQueryable = false,
                             Type acceptType = null)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            this.Node = node;
            this.Request = request ?? new PomonaRequest(node.RelativePath, node.RelativePath);
            this.expandedPaths = expandedPaths ?? GetExpandedPathsFromRequest(this.Request.Headers, Query);
            this.executeQueryable = executeQueryable;
            this.acceptType = acceptType;
        }


        public PomonaRequest Request { get; private set; }

        public Type AcceptType
        {
            get { return this.acceptType; }
        }

        public bool ExecuteQueryable
        {
            get { return this.executeQueryable; }
        }

        // TODO: Clean up this constructor!!

        public string ExpandedPaths
        {
            get { return this.expandedPaths; }
        }

        public RequestHeaders Headers
        {
            get { return this.Request.Headers; }
        }

        public HttpMethod Method
        {
            get { return this.Request.Method; }
        }

        public UrlSegment Node { get; set; }

        public dynamic Query
        {
            get { return Request.Query; }
        }

        public Route Route
        {
            get { return this.Node.Route; }
        }

        public IPomonaSession Session
        {
            get { return this.Node.Session; }
        }

        public TypeMapper TypeMapper
        {
            get { return this.Node.Session.TypeMapper; }
        }

        public string Url
        {
            get { return this.Request.Url; }
        }


        public object Bind(TypeSpec type = null, object patchedObject = null)
        {
            if (this.deserializedBody == null)
            {
                if (this.Request.Body == null)
                    throw new InvalidOperationException("No http body to deserialize.");

                if (Method == HttpMethod.Patch)
                {
                    patchedObject = patchedObject
                                    ?? Node.Session.Dispatch(new PomonaContext(this.Node, executeQueryable : true))
                                        .Entity;
                    if (patchedObject != null)
                        type = TypeMapper.GetClassMapping(patchedObject.GetType());
                }

                this.deserializedBody = Deserialize(type as TransformedType, patchedObject);
            }
            return this.deserializedBody;
        }


        public bool TryBindAsType(TypeSpec type, out object form)
        {
            form = this.deserializedBody;
            if (form == null || !type.Type.IsInstanceOfType(form))
                form = Deserialize(type as TransformedType, null);
            if (type.Type.IsInstanceOfType(form))
            {
                this.deserializedBody = form;
                return true;
            }
            return false;
        }


        private static string GetExpandedPathsFromRequest(RequestHeaders requestHeaders, DynamicDictionary query)
        {
            var expansions = requestHeaders["X-Pomona-Expand"];
            if (query["$expand"].HasValue)
                expansions = expansions.Append((string)query["$expand"]);
            var expandedPathsTemp = string.Join(",", expansions);
            return expandedPathsTemp;
        }


        private object Deserialize(TransformedType expectedBaseType, object patchedObject = null)
        {
            if (!this.Request.Body.CanSeek)
            {
                var memStream = new MemoryStream();
                this.Request.Body.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                this.Request.Body = memStream;
            }
            if (this.Request.Body.Position != 0)
                this.Request.Body.Seek(0, SeekOrigin.Begin);

            using (var textReader = new StreamReader(this.Request.Body))
            {
                return Session.GetInstance<ITextDeserializer>().Deserialize(textReader,
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