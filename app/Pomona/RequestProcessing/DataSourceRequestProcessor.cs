#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using System.Reflection;

using Mono.Cecil;

using Nancy;

using Newtonsoft.Json.Serialization;

using Pomona.Common;
using Pomona.Common.Internals;

using ResourceType = Pomona.Common.TypeSystem.ResourceType;

namespace Pomona.RequestProcessing
{
    public class DataSourceRequestProcessor : IPomonaRequestProcessor
    {
        private readonly IPomonaDataSource dataSource;
        
        private readonly Func<Type, DataSourceRequestProcessor, object, PomonaRequest, PomonaResponse> patchMethod =
            GenericInvoker.Instance<DataSourceRequestProcessor>().CreateFunc1<object, PomonaRequest, PomonaResponse>(
                x => x.Patch<object>(null, null));

        private readonly Func<Type, DataSourceRequestProcessor, object, PomonaRequest, PomonaResponse> postMethod =
            GenericInvoker.Instance<DataSourceRequestProcessor>().CreateFunc1<object, PomonaRequest, PomonaResponse>(
                x => x.Post<object>(null, null));


        public DataSourceRequestProcessor(IPomonaDataSource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.dataSource = dataSource;
        }


        private PomonaResponse ProcessQueryableNodeCallToHandler(PomonaRequest request, QueryableNode queryableNode)
        {
            ResourceType resourceType = queryableNode.ItemResourceType;
            if (!resourceType.IsRootResource)
            {
                var parentType = resourceType.ParentResourceType;
                // First attempt to locate handler with signature Post(ParentType, ResourceType)
                var method = dataSource.GetType().GetMethod("Post",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[] { parentType, resourceType },
                    null);

                if (method != null)
                {
                    var form = request.Bind(resourceType);
                    var result =  method.Invoke(dataSource, new object[] { queryableNode.Parent.Value, form });
                    if (!(result is PomonaResponse))
                    {
                        return new PomonaResponse(result, HttpStatusCode.Created, request.ExpandedPaths);
                    }
                    return (PomonaResponse)result;
                }
            }
            return null;
        }


        private PomonaResponse ProcessQueryableNodeCallToDataSource(PomonaRequest request, QueryableNode queryableNode)
        {
            var form = request.Bind();
            return postMethod(form.GetType(), this, form, request);
        }

        public virtual PomonaResponse Process(PomonaRequest request)
        {
            var queryableNode = request.Node as QueryableNode;
            var resourceNode = request.Node as ResourceNode;

            if (resourceNode != null)
            {
                return ProcessResourceNode(request, resourceNode);
            }
            if (queryableNode != null)
            {
                return ProcessQueryableNode(request, queryableNode);
            }

            return null;
        }


        private PomonaResponse ProcessQueryableNode(PomonaRequest request, QueryableNode queryableNode)
        {
            switch (request.Method)
            {
                case HttpMethod.Post:
                    return PostToCollection(request, queryableNode);
                default:
                    return null;
            }
        }


        private PomonaResponse ProcessResourceNode(PomonaRequest request, ResourceNode resourceNode)
        {
            switch (request.Method)
            {
                case HttpMethod.Post:
                    return PostToResourceNode(request, resourceNode);
                case HttpMethod.Patch:
                    return PatchResourceNode(request);
                default:
                    return null;
            }
        }


        private PomonaResponse PatchResourceNode(PomonaRequest request)
        {
            var patchedObject = request.Bind();
            return this.patchMethod(patchedObject.GetType(), this, patchedObject, request);
        }


        private PomonaResponse PostToResourceNode(PomonaRequest request, ResourceNode resourceNode)
        {
            // Find post to resource methods
            var form = request.Bind();
            var method = this.dataSource.GetType().GetMethod("Post",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { resourceNode.Type, form.GetType() },
                null);

            if (method == null)
            {
                throw new PomonaException("Method Post to resource not allowed for resource type",
                    null,
                    HttpStatusCode.BadRequest);
            }
            var result = method.Invoke(this.dataSource, new[] { resourceNode.Value, form });
            return new PomonaResponse(result);
        }


        private PomonaResponse PostToCollection(PomonaRequest request, QueryableNode queryableNode)
        {
            return ProcessQueryableNodeCallToHandler(request, queryableNode)
                   ?? ProcessQueryableNodeCallToDataSource(request, queryableNode);
        }


        public PomonaResponse Patch<T>(T form, PomonaRequest request)
            where T : class
        {
            return new PomonaResponse(this.dataSource.Patch(form), HttpStatusCode.OK, request.ExpandedPaths);
        }


        public PomonaResponse Post<T>(T form, PomonaRequest request)
            where T : class
        {
            return new PomonaResponse(this.dataSource.Post(form), HttpStatusCode.Created, request.ExpandedPaths);
        }
    }
}