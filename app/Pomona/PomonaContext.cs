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
        private readonly bool handleException;
        private object deserializedBody;


        public PomonaContext(UrlSegment node,
                             PomonaRequest request = null,
                             string expandedPaths = null,
                             bool executeQueryable = false,
                             bool handleException = true,
                             Type acceptType = null)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            Node = node;
            Request = request ?? new PomonaRequest(node.RelativePath, node.RelativePath);
            this.expandedPaths = expandedPaths ?? GetExpandedPathsFromRequest(Request.Headers, Query);
            this.executeQueryable = executeQueryable;
            this.handleException = handleException;
            this.acceptType = acceptType;
        }


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

        public bool HandleException
        {
            get { return this.handleException; }
        }

        public RequestHeaders Headers
        {
            get { return Request.Headers; }
        }

        public HttpMethod Method
        {
            get { return Request.Method; }
        }

        public UrlSegment Node { get; set; }

        public dynamic Query
        {
            get { return Request.Query; }
        }

        public PomonaRequest Request { get; private set; }

        public Route Route
        {
            get { return Node.Route; }
        }

        public IPomonaSession Session
        {
            get { return Node.Session; }
        }

        public TypeMapper TypeMapper
        {
            get { return Node.Session.TypeMapper; }
        }

        public string Url
        {
            get { return Request.Url; }
        }


        public object Bind(TypeSpec type = null, object patchedObject = null)
        {
            if (this.deserializedBody == null)
            {
                if (Request.Body == null)
                    throw new InvalidOperationException("No http body to deserialize.");

                if (Method == HttpMethod.Patch)
                {
                    patchedObject = patchedObject
                                    ?? Node.Session.Dispatch(new PomonaContext(Node, executeQueryable : true))
                                           .Entity;
                    if (patchedObject != null)
                        type = TypeMapper.FromType(patchedObject.GetType());
                }

                this.deserializedBody = Deserialize(type as StructuredType, patchedObject);
            }
            return this.deserializedBody;
        }


        public bool TryBindAsType(TypeSpec type, out object form)
        {
            form = this.deserializedBody;
            if (form == null || !type.Type.IsInstanceOfType(form))
                form = Deserialize(type as StructuredType, null);
            if (type.Type.IsInstanceOfType(form))
            {
                this.deserializedBody = form;
                return true;
            }
            return false;
        }


        private object Deserialize(StructuredType expectedBaseType, object patchedObject = null)
        {
            if (!Request.Body.CanSeek)
            {
                var memStream = new MemoryStream();
                Request.Body.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                Request.Body = memStream;
            }
            if (Request.Body.Position != 0)
                Request.Body.Seek(0, SeekOrigin.Begin);

            using (var textReader = new StreamReader(Request.Body))
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


        private static string GetExpandedPathsFromRequest(RequestHeaders requestHeaders, DynamicDictionary query)
        {
            var expansions = requestHeaders["X-Pomona-Expand"];
            if (query["$expand"].HasValue)
                expansions = expansions.Append((string)query["$expand"]);
            var expandedPathsTemp = string.Join(",", expansions);
            return expandedPathsTemp;
        }
    }
}