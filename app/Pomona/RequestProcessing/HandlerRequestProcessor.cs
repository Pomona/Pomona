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
using Pomona.Common.TypeSystem;

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
        private static readonly ConcurrentDictionary<string, Method> handlerMethodCache =
            new ConcurrentDictionary<string, Method>();


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


        public IEnumerable<Method> GetHandlerMethods(TypeMapper mapper)
        {
            return
                typeof(THandler).GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(
                    x => new Method(x, mapper));
        }


        private PomonaResponse DeleteResource(PomonaRequest request, ResourceNode resourceNode)
        {
            var resource = resourceNode.Value;
            var method = GetHandlerMethod(request.Method, resource.GetType(), PathNodeType.Resource, request.TypeMapper);

            return InvokeAndWrap(request, method);
        }


        private Method GetHandlerMethod(HttpMethod method, Type resourceType, PathNodeType nodeType, TypeMapper mapper)
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
            Method method,
            HttpStatusCode? statusCode = null)
        {
            // Continue to next request processor if method was not found.
            if (method == null)
                return null;

            var handler = request.NancyContext.Resolve(typeof(THandler));
            var result = method.Invoke(handler, request);
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


        private Method ResolveHandlerMethod(HttpMethod method,
            Type resourceType,
            PathNodeType nodeType,
            TypeMapper mapper)
        {
            var typeSpec = mapper.GetClassMapping(resourceType);
            var matches = GetHandlerMethods(mapper).Where(x => x.Match(method, nodeType, typeSpec)).ToList();
            if (matches.Count > 1)
                throw new NotImplementedException("Method overload resolution not implemented");
            return matches.FirstOrDefault();
        }

        #region Nested type: Method

        public class Method
        {
            private readonly MethodInfo methodInfo;

            private readonly System.Lazy<IList<Type>> parameterTypes;
            private readonly System.Lazy<IList<Parameter>> parameters;
            private readonly TypeMapper typeMapper;


            public Method(MethodInfo methodInfo, TypeMapper typeMapper)
            {
                if (methodInfo == null)
                    throw new ArgumentNullException("methodInfo");
                if (typeMapper == null)
                    throw new ArgumentNullException("typeMapper");
                this.methodInfo = methodInfo;
                this.typeMapper = typeMapper;
                this.parameters =
                    new System.Lazy<IList<Parameter>>(
                        () => methodInfo.GetParameters().Select(x => new Parameter(x, this)).ToList());
                this.parameterTypes = new System.Lazy<IList<Type>>(() => Parameters.MapList(x => x.Type));
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

            public IList<Parameter> Parameters
            {
                get { return this.parameters.Value; }
            }

            public Type ReturnType
            {
                get { return this.methodInfo.ReturnType; }
            }

            public TypeMapper TypeMapper
            {
                get { return this.typeMapper; }
            }


            public object Invoke(object target, PomonaRequest request)
            {
                object resourceArg = null;

                var httpMethod = request.Method;

                if (request.Node.NodeType == PathNodeType.Resource)
                {
                    switch (httpMethod)
                    {
                        case HttpMethod.Get:
                        {
                            var resourceNode = (ResourceNode)request.Node;
                            object parsedId;
                            if (!resourceNode.Name.TryParse(resourceNode.Type.PrimaryId.PropertyType, out parsedId))
                                throw new NotImplementedException("What to do when ID won't parse here??");

                            resourceArg = parsedId;
                        }
                            break;
                        case HttpMethod.Patch:
                        case HttpMethod.Post:
                            resourceArg = request.Bind();
                            break;
                        default:
                            resourceArg = request.Node.Value;
                            break;
                    }
                }
                else if (request.Node.NodeType == PathNodeType.Collection)
                {
                    switch (httpMethod)
                    {
                        case HttpMethod.Post:
                            resourceArg = request.Bind();
                            break;
                    }
                }
                return this.methodInfo.Invoke(target, new object[] { resourceArg });
            }


            public bool Match(HttpMethod method, PathNodeType nodeType, TypeSpec resourceType)
            {
                if (!this.methodInfo.Name.StartsWith(method.ToString()))
                    return false;
                switch (nodeType)
                {
                    case PathNodeType.Collection:
                        return MatchCollectionNodeRequest(method, (ResourceType)resourceType);
                    case PathNodeType.Resource:
                        return MatchResourceNodeRequest(method, (ResourceType)resourceType);
                }
                return false;
            }


            public bool NameStartsWith(string start)
            {
                return Name.ToLowerInvariant().StartsWith(start.ToLowerInvariant());
            }


            public bool ReturnsType(Type returnType)
            {
                return returnType.IsAssignableFrom(this.methodInfo.ReturnType);
            }


            private bool MatchCollectionNodeRequest(HttpMethod method, ResourceType resourceType)
            {
                switch (method)
                {
                    case HttpMethod.Post:
                        return MatchMethodTakingResourceObject(resourceType);
                }
                return false;
            }


            private bool MatchMethodTakingResourceId(ResourceType resourceType)
            {
                if (this.methodInfo.ReturnType != resourceType.Type)
                    return false;

                var idParam = Parameters.SingleOrDefault(x => x.Type == resourceType.PrimaryId.PropertyType.Type);
                return idParam != null;
            }


            private bool MatchMethodTakingResourceObject(ResourceType resourceType)
            {
                var resourceTypeParam = Parameters.Where(x => x.IsResource && x.Type.IsAssignableFrom(resourceType));
                return resourceTypeParam.Any();
            }


            private bool MatchResourceNodeRequest(HttpMethod httpMethod, ResourceType resourceType)
            {
                switch (httpMethod)
                {
                    case HttpMethod.Delete:
                        return MatchMethodTakingResourceObject(resourceType);
                    case HttpMethod.Patch:
                        return MatchMethodTakingResourceObject(resourceType);
                    case HttpMethod.Get:
                        return MatchMethodTakingResourceId(resourceType);
                }
                return false;
            }
        }

        #endregion

        #region Nested type: Parameter

        public class Parameter
        {
            private readonly Method method;
            private readonly ParameterInfo parameterInfo;
            private TypeSpec typeSpec;


            public Parameter(ParameterInfo parameterInfo, Method method)
            {
                this.parameterInfo = parameterInfo;
                this.method = method;
            }


            public bool IsResource
            {
                get { return TypeSpec is ResourceType; }
            }

            public Type Type
            {
                get { return this.parameterInfo.ParameterType; }
            }

            public TypeSpec TypeSpec
            {
                get
                {
                    this.method.TypeMapper.TryGetTypeSpec(this.parameterInfo.ParameterType, out this.typeSpec);
                    return this.typeSpec;
                }
            }
        }

        #endregion
    }
}