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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public class HandlerMethod
    {
        private readonly MethodInfo methodInfo;
        private readonly Lazy<IList<HandlerParameter>> parameters;
        private readonly Lazy<IList<Type>> parameterTypes;
        private readonly TypeMapper typeMapper;


        public HandlerMethod(MethodInfo methodInfo, TypeMapper typeMapper)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            this.methodInfo = methodInfo;
            this.typeMapper = typeMapper;
            this.parameters =
                new Lazy<IList<HandlerParameter>>(
                    () => methodInfo.GetParameters().Select(x => new HandlerParameter(x, this)).ToList());
            this.parameterTypes = new Lazy<IList<Type>>(() => Parameters.MapList(x => x.Type));
        }


        public MethodInfo MethodInfo
        {
            get { return this.methodInfo; }
        }

        public string Name
        {
            get { return this.methodInfo.Name; }
        }

        public IList<HandlerParameter> Parameters
        {
            get { return this.parameters.Value; }
        }

        public IList<Type> ParameterTypes
        {
            get { return this.parameterTypes.Value; }
        }

        public Type ReturnType
        {
            get { return this.methodInfo.ReturnType; }
        }

        public TypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }


        public RouteAction Match(HttpMethod method, PathNodeType nodeType, TypeSpec resourceType)
        {
            switch (nodeType)
            {
                case PathNodeType.Collection:
                    return MatchCollectionNodeRequest(method, (ResourceType)resourceType);
                case PathNodeType.Resource:
                    if (!this.methodInfo.Name.StartsWith(method.ToString()))
                        return null;
                    return MatchResourceNodeRequest(method, (ResourceType)resourceType);
            }
            return null;
        }


        public bool NameStartsWith(HttpMethod method)
        {
            return Name.StartsWith(method.ToString());
        }


        public bool ReturnsType(Type returnType)
        {
            return returnType.IsAssignableFrom(this.methodInfo.ReturnType);
        }


        private RouteAction MatchCollectionNodeRequest(HttpMethod method, ResourceType resourceType)
        {
            switch (method)
            {
                case HttpMethod.Post:
                    return NameStartsWith(method) ? MatchMethodTakingForm(resourceType) : null;
                case HttpMethod.Get:
                    return MatchMethodReturningQueryable(resourceType);
            }
            return null;
        }


        private RouteAction MatchMethodReturningQueryable(ResourceType resourceType)
        {
            // Check that the method is called "Get", "Query", "Get<TypeName>s" or "Query<TypeName>s".
            if (!this.methodInfo.Name.Equals("Get") && !this.methodInfo.Name.Equals("Query") &&
                !this.methodInfo.Name.Equals("Get" + resourceType.PluralName) &&
                !this.methodInfo.Name.Equals("Query" + resourceType.PluralName))
                return null;

            // Check that the it takes a parameter of type Parent if the type is a child resource of Parent.
            if (resourceType.ParentResourceType != null)
            {
                var parentParameter = this.methodInfo.GetParameters();
                if (parentParameter.Length != 1 ||
                    parentParameter[0].ParameterType != resourceType.ParentResourceType.Type)
                    return null;
            }

            // Check that it returns an IQueryable<Object>.
            if (!typeof(IQueryable<>).MakeGenericType(resourceType.Type).IsAssignableFrom(this.methodInfo.ReturnType))
                return null;

            return new DefaultHandlerMethodInvoker(this);
        }


        private RouteAction MatchMethodTakingExistingResource(ResourceType resourceType)
        {
            var existingTypeParam = Parameters.SingleOrDefaultIfMultiple(x => x.Type.IsAssignableFrom(resourceType));
            if (existingTypeParam == null || Parameters.Skip(existingTypeParam.Position + 1).Any(x => x.IsTransformedType))
                return null;

            return new HandlerMethodTakingExistingResource(this, resourceType);
        }


        private RouteAction MatchMethodTakingExistingResourceAndForm(ResourceType resourceType)
        {
            var existingTypeParam = Parameters.SingleOrDefaultIfMultiple(x => x.Type.IsAssignableFrom(resourceType));
            if (existingTypeParam == null)
                return null;

            // Find exactly one form parameter after resource arg:
            var formParam = Parameters.Skip(existingTypeParam.Position + 1).SingleOrDefaultIfMultiple(x => x.IsTransformedType);
            if (formParam == null)
                return null;

            var resourceTypeParam = Parameters.Where(x => x.IsResource && x.Type.IsAssignableFrom(resourceType));
            return resourceTypeParam.Any() ? new HandlerMethodTakingFormInvoker(this, formParam, existingTypeParam) : null;
        }


        private RouteAction MatchMethodTakingForm(ResourceType resourceType)
        {
            var resourceTypeParam = Parameters.LastOrDefault(x => x.IsTransformedType);

            return (resourceTypeParam != null && resourceTypeParam.Type.IsAssignableFrom(resourceType))
                ? new HandlerMethodTakingFormInvoker(this, resourceTypeParam)
                : null;
        }


        private RouteAction MatchMethodTakingPatchedExistingResource(ResourceType resourceType)
        {
            var existingTypeParam = Parameters.SingleOrDefaultIfMultiple(x => x.Type.IsAssignableFrom(resourceType));
            if (existingTypeParam == null || Parameters.Skip(existingTypeParam.Position + 1).Any(x => x.IsTransformedType))
                return null;

            return new HandlerMethodTakingPatchedResource(this, resourceType);
        }


        private RouteAction MatchMethodTakingResourceId(ResourceType resourceType)
        {
            if (this.methodInfo.ReturnType != resourceType.Type)
                return null;

            var idParam = Parameters.SingleOrDefault(x => x.Type == resourceType.PrimaryId.PropertyType.Type);
            if (idParam != null)
                return new HandlerMethodTakingResourceId(this);
            return null;
        }


        private RouteAction MatchResourceNodeRequest(HttpMethod httpMethod, ResourceType resourceType)
        {
            if (!NameStartsWith(httpMethod))
                return null;

            switch (httpMethod)
            {
                case HttpMethod.Delete:
                    return MatchMethodTakingExistingResource(resourceType);
                case HttpMethod.Patch:
                    return MatchMethodTakingPatchedExistingResource(resourceType);
                case HttpMethod.Get:
                    return MatchMethodTakingResourceId(resourceType);
                case HttpMethod.Post:
                    return MatchMethodTakingExistingResourceAndForm(resourceType);
            }
            return null;
        }
    }
}