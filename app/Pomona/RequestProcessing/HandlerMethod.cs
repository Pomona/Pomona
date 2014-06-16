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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Nancy;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
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

        public HandlerMethodInvoker Match(HttpMethod method, PathNodeType nodeType, TypeSpec resourceType)
        {
            if (!methodInfo.Name.StartsWith(method.ToString()))
                return null;
            switch (nodeType)
            {
                case PathNodeType.Collection:
                    return MatchCollectionNodeRequest(method, (ResourceType) resourceType);
                case PathNodeType.Resource:
                    return MatchResourceNodeRequest(method, (ResourceType) resourceType);
            }
            return null;
        }


        public bool NameStartsWith(string start)
        {
            return Name.ToLowerInvariant().StartsWith(start.ToLowerInvariant());
        }

        public bool ReturnsType(Type returnType)
        {
            return returnType.IsAssignableFrom(methodInfo.ReturnType);
        }


        private HandlerMethodInvoker MatchCollectionNodeRequest(HttpMethod method, ResourceType resourceType)
        {
            switch (method)
            {
                case HttpMethod.Post:
                    return MatchMethodTakingResourceObject(resourceType);
                case HttpMethod.Get:
                    return MatchMethodReturningQueryable(resourceType);
            }
            return null;
        }

        private HandlerMethodInvoker MatchMethodReturningQueryable(ResourceType resourceType)
        {
            // Check that the method is called "Get", "Query", "Get<TypeName>s" or "Query<TypeName>s".
            if (!methodInfo.Name.Equals("Get") && !methodInfo.Name.Equals("Query") &&
                !methodInfo.Name.Equals("Get" + resourceType.PluralName) &&
                !methodInfo.Name.Equals("Query" + resourceType.PluralName))
                return null;

            // Check that the it takes a parameter of type Parent if the type is a child resource of Parent.
            if (resourceType.ParentResourceType != null)
            {
                ParameterInfo[] parentParameter = methodInfo.GetParameters();
                if (parentParameter.Length != 1 ||
                    parentParameter[0].ParameterType != resourceType.ParentResourceType.Type)
                    return null;
            }

            // Check that it returns an IQueryable<Object>.
            if (!typeof (IQueryable<>).MakeGenericType(resourceType.Type).IsAssignableFrom(methodInfo.ReturnType))
                return null;

            return new DefaultHandlerMethodInvoker(this);
        }

        private HandlerMethodInvoker MatchMethodTakingResourceId(ResourceType resourceType)
        {
            if (methodInfo.ReturnType != resourceType.Type)
                return null;

            var idParam = Parameters.SingleOrDefault(x => x.Type == resourceType.PrimaryId.PropertyType.Type);
            if (idParam != null)
            {
                return new DefaultHandlerMethodInvoker(this);
            }
            return null;
        }


        private HandlerMethodInvoker MatchMethodTakingResourceObject(ResourceType resourceType)
        {
            var resourceTypeParam = Parameters.Where(x => x.IsResource && x.Type.IsAssignableFrom(resourceType));
            return resourceTypeParam.Any() ? new DefaultHandlerMethodInvoker(this) : null;
        }


        private HandlerMethodInvoker MatchResourceNodeRequest(HttpMethod httpMethod, ResourceType resourceType)
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
            return null;
        }
    }
}