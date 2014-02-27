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
using System.Linq;
using System.Reflection;

using Nancy;

using Pomona.Common;
using Pomona.TypeSystem;

namespace Pomona.RequestProcessing
{
    public abstract class HandlerRequestProcessor : IPomonaRequestProcessor
    {
        public abstract PomonaResponse Process(PomonaRequest request);


        public static HandlerRequestProcessor Create(Type type)
        {
            return
                (HandlerRequestProcessor)
                    Activator.CreateInstance(typeof(HandlerRequestProcessor<>).MakeGenericType(type));
        }
    }

    public class HandlerRequestProcessor<THandler> : HandlerRequestProcessor
    {
        public override PomonaResponse Process(PomonaRequest request)
        {
            var resourceNode = request.Node as ResourceNode;
            if (resourceNode != null)
                return ProcessResourceNode(request, resourceNode);
            var queryableNode = request.Node as QueryableNode;
            if (queryableNode != null)
                return ProcessQueryableNode(request, queryableNode);
            return null;
        }


        private static MethodInfo GetResourceHandlerMethod(HttpMethod method, Type formType)
        {
            return GetResourceHandlerMethod(method.ToString(), formType);
        }


        private static MethodInfo GetResourceHandlerMethod(string methodName, Type formType)
        {
            var method =
                typeof(THandler).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.Name.ToLowerInvariant().StartsWith(methodName.ToLowerInvariant()))
                    .Select(x => new { m = x, p = x.GetParameters() })
                    .Where(x => x.p.Length == 1)
                    .Select(x => new { x.m, pType = x.p[0].ParameterType })
                    .Where(x => x.pType.IsAssignableFrom(formType))
                    .OrderByDescending(x => x.pType, new SubclassComparer())
                    .Select(x => x.m)
                    .FirstOrDefault();
            return method;
        }


        private PomonaResponse DeleteResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var resource = resourceNode.Value;
            var method = GetResourceHandlerMethod(request.Method, resource.GetType());

            if (method == null)
                return null;

            var handler = request.NancyContext.Resolve(typeof(THandler));
            var result = method.Invoke(handler, new[] { resource });
            var resultAsResponse = result as PomonaResponse;
            if (resultAsResponse != null)
                return resultAsResponse;

            return new PomonaResponse(PomonaResponse.NoBodyEntity, HttpStatusCode.NoContent);
        }


        private PomonaResponse PostToCollection(PomonaRequest request, QueryableNode queryableNode)
        {
            var form = request.Bind();
            var method = GetResourceHandlerMethod(request.Method, form.GetType());

            if (method == null)
                return null;

            var handler = request.NancyContext.Resolve(typeof(THandler));
            var result = method.Invoke(handler, new object[] { form });
            var resultAsResponse = result as PomonaResponse;
            if (resultAsResponse != null)
                return resultAsResponse;

            return new PomonaResponse(result, HttpStatusCode.Created, request.ExpandedPaths);
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
                case HttpMethod.Delete:
                    return DeleteResource(request, resourceNode);
                default:
                    return null;
            }
        }
    }
}