#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
        private object deserializedBody;


        public PomonaContext(UrlSegment node,
                             PomonaRequest request = null,
                             string expandedPaths = null,
                             bool executeQueryable = false,
                             bool handleException = true,
                             Type acceptType = null)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            Node = node;
            Request = request ?? new PomonaRequest(node.RelativePath, node.RelativePath);
            ExpandedPaths = expandedPaths ?? GetExpandedPathsFromRequest(Request.Headers, Query);
            ExecuteQueryable = executeQueryable;
            HandleException = handleException;
            AcceptType = acceptType;
        }


        public Type AcceptType { get; }

        public bool ExecuteQueryable { get; }

        // TODO: Clean up this constructor!!

        public string ExpandedPaths { get; }

        public bool HandleException { get; }

        public RequestHeaders Headers => Request.Headers;

        public HttpMethod Method => Request.Method;

        public UrlSegment Node { get; set; }

        public dynamic Query => Request.Query;

        public PomonaRequest Request { get; private set; }

        public Route Route => Node.Route;

        public IPomonaSession Session => Node.Session;

        public TypeMapper TypeMapper => Node.Session.TypeMapper;

        public string Url => Request.Url;


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
                var deserializer = Session.GetInstance<ITextDeserializer>();
                var options = new DeserializeOptions()
                {
                    Target = patchedObject,
                    ExpectedBaseType = expectedBaseType,
                    TargetNode = Node
                };
                return deserializer.Deserialize(textReader, options);
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

