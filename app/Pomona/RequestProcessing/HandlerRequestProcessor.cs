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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Nancy;

using Pomona.Common;
using Pomona.Common.Internals;
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
        private static readonly ConcurrentDictionary<string, MethodInfo> handlerMethodCache =
            new ConcurrentDictionary<string, MethodInfo>();


        public static IEnumerable<Method> GetHandlerMethods()
        {
            return typeof(THandler).GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(x => new Method(x));
        }


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


        private static MethodInfo GetHandlerMethodTakingId(string methodName, Type idType, Type resourceType)
        {
            var cacheKey = string.Format("GetHandlerMethodTakingId::{0}::{1}", idType.FullName, resourceType.FullName);

            return handlerMethodCache.GetOrAdd(cacheKey,
                k => GetHandlerMethods()
                    .Where(x => x.NameStartsWith(methodName) && x.ReturnsType(resourceType))
                    .Where(x => x.Parameters.Count == 1 && x.ParameterTypes[0] == idType)
                    .OrderBy(x => x.ReturnType, new SubclassComparer())
                    .Select(x => x.MethodInfo)
                    .FirstOrDefault());
        }


        private static MethodInfo GetHandlerMethodTakingResource(HttpMethod method, Type formType)
        {
            return GetHandlerMethodTakingResource(method.ToString(), formType);
        }


        private static MethodInfo GetHandlerMethodTakingResource(string methodName, Type formType)
        {
            var cacheKey = string.Format("GetHandlerMethodTakingResource::{0}::{1}", methodName, formType.FullName);
            return handlerMethodCache.GetOrAdd(cacheKey,
                k => GetHandlerMethods()
                    .Where(x => x.Name.ToLowerInvariant().StartsWith(methodName.ToLowerInvariant()))
                    .Where(x => x.Parameters.Count == 1)
                    .Where(x => x.ParameterTypes[0].IsAssignableFrom(formType))
                    .OrderByDescending(x => x.ParameterTypes[0], new SubclassComparer())
                    .Select(x => x.MethodInfo)
                    .FirstOrDefault());
        }


        private PomonaResponse DeleteResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var resource = resourceNode.Value;
            var method = GetHandlerMethodTakingResource(request.Method, resource.GetType());

            return InvokeHandlerAndWrapResponse(request, method, new object[] { resource });
        }


        private PomonaResponse PatchResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var resource = request.Bind(); // Will patch resourceNode.Value by default.
            var method = GetHandlerMethodTakingResource(request.Method, resource.GetType());

            return InvokeHandlerAndWrapResponse(request, method, new object[] { resource });
        }


        private PomonaResponse GetResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var method = GetHandlerMethodTakingId(request.Method.ToString(),
                resourceNode.Type.PrimaryId.PropertyType,
                resourceNode.Type);


            // If id is not parseable to type of primary id pass on null, and 404 later if no other handlers found.
            object parsedId;
            if (!resourceNode.Name.TryParse(resourceNode.Type.PrimaryId.PropertyType, out parsedId))
                return null;

            return InvokeHandlerAndWrapResponse(request,
                method,
                new[] { parsedId });
        }


        private PomonaResponse InvokeHandlerAndWrapResponse(PomonaRequest request,
            MethodInfo method,
            object[] methodParams,
            HttpStatusCode? statusCode = null)
        {
            // Continue to next request processor if method was not found.
            if (method == null)
                return null;

            var handler = request.NancyContext.Resolve(typeof(THandler));
            var result = method.Invoke(handler, methodParams);
            var resultAsResponse = result as PomonaResponse;
            if (resultAsResponse != null)
                return resultAsResponse;

            var responseBody = result;
            if (method.ReturnType == typeof(void))
                responseBody = PomonaResponse.NoBodyEntity;

            if (responseBody == PomonaResponse.NoBodyEntity)
                statusCode = HttpStatusCode.NoContent;

            return new PomonaResponse(responseBody, statusCode ?? HttpStatusCode.OK, request.ExpandedPaths);
        }


        private PomonaResponse PostToCollection(PomonaRequest request, ResourceCollectionNode collectionNode)
        {
            var form = request.Bind();
            var method = GetHandlerMethodTakingResource(request.Method, form.GetType());

            return InvokeHandlerAndWrapResponse(request,
                method,
                new object[] { form },
                statusCode : HttpStatusCode.Created);
        }


        private PomonaResponse ProcessCollectionNode(PomonaRequest request, ResourceCollectionNode collectionNode)
        {
            switch (request.Method)
            {
                case HttpMethod.Post:
                    return PostToCollection(request, collectionNode);

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
                case HttpMethod.Get:
                    return GetResource(request, resourceNode);
                case HttpMethod.Patch:
                    return PatchResource(request, resourceNode);
                default:
                    return null;
            }
        }

        #region Nested type: Method

        public class Method
        {
            private readonly MethodInfo methodInfo;

            private readonly Lazy<IList<Type>> parameterTypes;
            private readonly Lazy<IList<ParameterInfo>> parameters;


            public Method(MethodInfo methodInfo)
            {
                if (methodInfo == null)
                    throw new ArgumentNullException("methodInfo");
                this.methodInfo = methodInfo;
                this.parameters = new Lazy<IList<ParameterInfo>>(methodInfo.GetParameters);
                this.parameterTypes = new Lazy<IList<Type>>(() => Parameters.MapList(x => x.ParameterType));
            }


            public MethodInfo MethodInfo
            {
                get { return this.methodInfo; }
            }

            public string Name
            {
                get { return this.methodInfo.Name; }
            }

            public IList<Type> ParameterTypes
            {
                get { return this.parameterTypes.Value; }
            }

            public IList<ParameterInfo> Parameters
            {
                get { return this.parameters.Value; }
            }

            public Type ReturnType
            {
                get { return this.methodInfo.ReturnType; }
            }


            public bool NameStartsWith(string start)
            {
                return Name.ToLowerInvariant().StartsWith(start.ToLowerInvariant());
            }


            public bool ReturnsType(Type returnType)
            {
                return returnType.IsAssignableFrom(this.methodInfo.ReturnType);
            }
        }

        #endregion
    }
}