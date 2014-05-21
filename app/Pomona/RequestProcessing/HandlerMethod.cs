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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{

    #region Nested type: HandlerMethod

    public class HandlerMethod
    {
        private readonly MethodInfo methodInfo;

        private readonly System.Lazy<IList<Type>> parameterTypes;
        private readonly System.Lazy<IList<HandlerParameter>> parameters;
        private readonly TypeMapper typeMapper;


        public HandlerMethod(MethodInfo methodInfo, TypeMapper typeMapper)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.methodInfo = methodInfo;
            this.typeMapper = typeMapper;
            parameters =
                new System.Lazy<IList<HandlerParameter>>(
                    () => methodInfo.GetParameters().Select(x => new HandlerParameter(x, this)).ToList());
            parameterTypes = new System.Lazy<IList<Type>>(() => Parameters.MapList(x => x.Type));
        }


        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        public string Name
        {
            get { return methodInfo.Name; }
        }

        public IList<Type> ParameterTypes
        {
            get { return parameterTypes.Value; }
        }

        public IList<HandlerParameter> Parameters
        {
            get { return parameters.Value; }
        }

        public Type ReturnType
        {
            get { return methodInfo.ReturnType; }
        }

        public TypeMapper TypeMapper
        {
            get { return typeMapper; }
        }


        public object Invoke(object target, PomonaRequest request)
        {
            var args = new object[Parameters.Count];
            object resourceArg = null;
            object resourceIdArg = null;
            var httpMethod = request.Method;

            ResourceType resourceType = null;
            ResourceType parentResourceType = null;
            Type[] returnTypeArgs = methodInfo.ReturnType.GetGenericArguments();

            if (request.Node.NodeType == PathNodeType.Resource)
            {
                switch (httpMethod)
                {
                    case HttpMethod.Get:
                    {
                        var resourceNode = (ResourceNode) request.Node;
                        object parsedId;
                        if (!resourceNode.Name.TryParse(resourceNode.Type.PrimaryId.PropertyType, out parsedId) &&
                            !typeof (IQueryable<Object>).IsAssignableFrom(methodInfo.ReturnType))
                            throw new NotImplementedException("What to do when ID won't parse here??");

                        resourceIdArg = parsedId;
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

            // If the method returns an IQueryable<Object> and takes a parent resource parameter,
            // check that the parameter is actually the parent resource type of the resouce type.
            if (typeof (IQueryable<Object>).IsAssignableFrom(methodInfo.ReturnType))
            {
                resourceType = request.Node.Type as ResourceType;
                if (resourceType != null)
                    parentResourceType = resourceType.ParentResourceType;
                if (parentResourceType != null)
                {
                    if (Parameters.Count != 1)
                    {
                        throw new PomonaException("Type " + resourceType.Name +
                                                  " has the parent resource type " +
                                                  parentResourceType.Name +
                                                  ", but no parent element was specified.");
                    }
                    else if (parentResourceType.Type != Parameters[0].Type)
                    {
                        throw new PomonaException("Type " + resourceType.Name +
                                                  " has the parent resource type " +
                                                  parentResourceType.Name +
                                                  ", but a parent element of type " + Parameters[0].Type.Name +
                                                  " was specified.");
                    }
                    else
                    {
                        args[0] = request.Node.Parent.Value;
                    }
                }
            }
            else
            {
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var p = Parameters[i];

                    if (p.IsResource && p.Type.IsInstanceOfType(resourceArg))
                        args[i] = resourceArg;
                    else if (p.Type == typeof (PomonaRequest))
                        args[i] = request;
                    else if (p.Type == typeof (NancyContext))
                        args[i] = request.NancyContext;
                    else if (p.Type == typeof (TypeMapper))
                        args[i] = request.TypeMapper;
                    else if (resourceIdArg != null && p.Type == resourceIdArg.GetType())
                        args[i] = resourceIdArg;
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "Unable to invoke handler {0}.{1}, don't know how to provide value for parameter {2}",
                                methodInfo.ReflectedType,
                                methodInfo.Name,
                                p.Name));
                    }
                }
            }
            return methodInfo.Invoke(target, args);
        }


        public bool Match(HttpMethod method, PathNodeType nodeType, TypeSpec resourceType)
        {
            if (!methodInfo.Name.StartsWith(method.ToString()))
                return false;
            switch (nodeType)
            {
                case PathNodeType.Collection:
                    return MatchCollectionNodeRequest(method, (ResourceType) resourceType);
                case PathNodeType.Resource:
                    return MatchResourceNodeRequest(method, (ResourceType) resourceType);
            }
            return false;
        }


        public bool NameStartsWith(string start)
        {
            return Name.ToLowerInvariant().StartsWith(start.ToLowerInvariant());
        }

        public bool ReturnsType(Type returnType)
        {
            return returnType.IsAssignableFrom(methodInfo.ReturnType);
        }


        private bool MatchCollectionNodeRequest(HttpMethod method, ResourceType resourceType)
        {
            switch (method)
            {
                case HttpMethod.Post:
                    return MatchMethodTakingResourceObject(resourceType);
                case HttpMethod.Get:
                    return MatchMethodReturningQueryable(resourceType);
            }
            return false;
        }

        private bool MatchMethodReturningQueryable(ResourceType resourceType)
        {
            // Check that the method is called "Get", "Query", "Get<TypeName>s" or "Query<TypeName>s".
            if (!methodInfo.Name.Equals("Get") && !methodInfo.Name.Equals("Query") &&
                !methodInfo.Name.Equals("Get" + resourceType.PluralName) &&
                !methodInfo.Name.Equals("Query" + resourceType.PluralName))
                return false;

            // Check that the it takes a parameter of type Parent if the type is a child resource of Parent.
            if (resourceType.ParentResourceType != null)
            {
                ParameterInfo[] parentParameter = methodInfo.GetParameters();
                if (parentParameter.Length != 1 ||
                    parentParameter[0].ParameterType != resourceType.ParentResourceType.Type)
                    return false;
            }

            // Check that it returns an IQueryable<Object>.
            if (!typeof (IQueryable<>).MakeGenericType(resourceType.Type).IsAssignableFrom(methodInfo.ReturnType))
                return false;

            return true;
        }

        private bool MatchMethodTakingResourceId(ResourceType resourceType)
        {
            if (methodInfo.ReturnType != resourceType.Type)
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
}