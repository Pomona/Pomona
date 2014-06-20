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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;
using Pomona.Common;

namespace Pomona.RequestProcessing
{
    public abstract class HandlerRequestProcessor : IPomonaRequestProcessor
    {
        public abstract PomonaResponse Process(PomonaRequest request);


        public static HandlerRequestProcessor Create(Type type)
        {
            return
                (HandlerRequestProcessor)
                    Activator.CreateInstance(typeof (HandlerRequestProcessor<>).MakeGenericType(type));
        }
    }

    public class HandlerRequestProcessor<THandler> : HandlerRequestProcessor
    {
        private static readonly ConcurrentDictionary<string, HandlerMethodInvoker> handlerMethodCache =
            new ConcurrentDictionary<string, HandlerMethodInvoker>();


        public override PomonaResponse Process(PomonaRequest request)
        {
            var resourceNode = request.Node as ResourceNode;
            if (resourceNode != null)
                return ProcessResourceNode(request, resourceNode);
            var collectionNode = request.Node as ResourceCollectionNode;
            if (collectionNode != null)
                return ProcessCollectionNode(request, collectionNode);
            return null;
        }


        public IEnumerable<HandlerMethod> GetHandlerMethods(TypeMapper mapper)
        {
            return
                typeof (THandler).GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(
                    x => new HandlerMethod(x, mapper));
        }


        private PomonaResponse DeleteResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var resource = resourceNode.Value;
            var method = GetHandlerMethod(request.Method, resource.GetType(), PathNodeType.Resource, request.TypeMapper);

            return InvokeAndWrap(request, method);
        }


        private HandlerMethodInvoker GetHandlerMethod(HttpMethod method, Type resourceType, PathNodeType nodeType,
            TypeMapper mapper)
        {
            var cacheKey = string.Format("{0}:{1}:{2}", method, resourceType.FullName, nodeType);
            return handlerMethodCache.GetOrAdd(cacheKey,
                k => ResolveHandlerMethod(method, resourceType, nodeType, mapper));
        }


        private PomonaResponse GetResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var method = GetHandlerMethod(request.Method, resourceNode.Type, resourceNode.NodeType, request.TypeMapper);

            return InvokeAndWrap(request, method);
        }


        private PomonaResponse InvokeAndWrap(PomonaRequest request,
            HandlerMethodInvoker method,
            HttpStatusCode? statusCode = null)
        {
            // Continue to next request processor if method was not found.
            if (method == null)
                return null;

            var handler = request.Context.Resolve(typeof (THandler));
            var result = method.Invoke(handler, request);
            var resultAsResponse = result as PomonaResponse;
            if (resultAsResponse != null)
                return resultAsResponse;

            IQueryable resultAsQueryable = result as IQueryable;
            if (resultAsQueryable != null && request.ExecuteQueryable)
            {
                return request.Node.GetQueryExecutor().ApplyAndExecute(resultAsQueryable, request.ParseQuery());
            }

            var responseBody = result;
            if (method.ReturnType == typeof (void))
                responseBody = PomonaResponse.NoBodyEntity;

            if (responseBody == PomonaResponse.NoBodyEntity)
                statusCode = HttpStatusCode.NoContent;

            return new PomonaResponse(responseBody, statusCode ?? HttpStatusCode.OK, request.ExpandedPaths);
        }


        private PomonaResponse PatchResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var resource = resourceNode.Value;
            var method = GetHandlerMethod(request.Method, resource.GetType(), PathNodeType.Resource, request.TypeMapper);

            return InvokeAndWrap(request, method);
        }


        private PomonaResponse PostToCollection(PomonaRequest request, ResourceCollectionNode collectionNode)
        {
            var form = request.Bind();
            var method = GetHandlerMethod(request.Method, form.GetType(), collectionNode.NodeType, request.TypeMapper);

            return InvokeAndWrap(request,
                method,
                statusCode: HttpStatusCode.Created);
        }


        private PomonaResponse ProcessCollectionNode(PomonaRequest request, ResourceCollectionNode collectionNode)
        {
            switch (request.Method)
            {
                case HttpMethod.Post:
                    return PostToCollection(request, collectionNode);
                case HttpMethod.Get:
                    return GetCollection(request, collectionNode);

                default:
                    return null;
            }
        }


        private PomonaResponse GetCollection(PomonaRequest request, ResourceCollectionNode collectionNode)
        {

            var method = GetHandlerMethod(request.Method, collectionNode.ItemResourceType, collectionNode.NodeType, request.TypeMapper);

            return InvokeAndWrap(request, method);
        }


        private PomonaResponse ProcessResourceNode(PomonaRequest request, ResourceNode resourceNode)
        {
            switch (request.Method)
            {
                case HttpMethod.Delete:
                    return DeleteResource(request, resourceNode);
                case HttpMethod.Get:
                    return GetResource(request, resourceNode);
                case HttpMethod.Patch:
                    return PatchResource(request, resourceNode);
                default:
                    return null;
            }
        }


        private HandlerMethodInvoker ResolveHandlerMethod(HttpMethod method,
            Type resourceType,
            PathNodeType nodeType,
            TypeMapper mapper)
        {
            var typeSpec = mapper.GetClassMapping(resourceType);
            var matches = GetHandlerMethods(mapper).Select(x => x.Match(method, nodeType, typeSpec)).Where(x => x != null).ToList();
            if (matches.Count > 1)
                throw new NotImplementedException("Method overload resolution not implemented");
            return matches.FirstOrDefault();
        }
    }
}