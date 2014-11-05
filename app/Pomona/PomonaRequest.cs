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
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona
{
    public class PomonaRequest
    {
        private readonly Type acceptType;
        private readonly bool executeQueryable;
        private readonly string expandedPaths;
        private readonly bool hasQuery;

        private readonly HttpMethod method;
        private readonly UrlSegment node;
        private readonly string url;
        private Stream body;
        private object deserializedBody;
        private RequestHeaders headers;


        public PomonaRequest(UrlSegment node,
                             HttpMethod method,
                             Stream body = null,
                             string expandedPaths = null,
                             string url = null,
                             RequestHeaders headers = null,
                             DynamicDictionary query = null,
                             bool executeQueryable = false,
                             bool hasQuery = false,
                             Type acceptType = null)
        {
            Query = query ?? new DynamicDictionary();
            this.node = node;
            this.body = body;
            this.headers = headers ?? new RequestHeaders(new Dictionary<string, IEnumerable<string>>());
            this.expandedPaths = expandedPaths ?? GetExpandedPathsFromRequest(this.headers, Query);
            this.url = url;
            this.method = method;
            this.executeQueryable = executeQueryable;
            this.hasQuery = hasQuery;
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

        public bool HasQuery
        {
            get { return this.hasQuery; }
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

        public UrlSegment Node
        {
            get { return this.node; }
        }

        public dynamic Query { get; set; }

        public Route Route
        {
            get { return this.node.Route; }
        }

        public IPomonaSession Session
        {
            get { return this.node.Session; }
        }

        public TypeMapper TypeMapper
        {
            get { return this.node.Session.TypeMapper; }
        }

        public string Url
        {
            get { return this.url; }
        }


        public object Bind(TypeSpec type = null, object patchedObject = null)
        {
            if (this.deserializedBody == null)
            {
                if (this.body == null)
                    throw new InvalidOperationException("No http body to deserialize.");

                if (Method == HttpMethod.Patch)
                {
                    patchedObject = patchedObject
                                    ?? Node.Session.Dispatch(new PomonaRequest(this.node,
                                                                               HttpMethod.Get,
                                                                               executeQueryable : true)).Entity;
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
            if (!this.body.CanSeek)
            {
                var memStream = new MemoryStream();
                this.body.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                this.body = memStream;
            }
            if (this.body.Position != 0)
                this.body.Seek(0, SeekOrigin.Begin);

            using (var textReader = new StreamReader(this.body))
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