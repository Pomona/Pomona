#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public class HandlerMethod
    {
        private readonly Lazy<IList<HandlerParameter>> parameters;
        private readonly Lazy<IList<Type>> parameterTypes;


        public HandlerMethod(MethodInfo methodInfo, TypeMapper typeMapper)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            MethodInfo = methodInfo;

            Type[] taskTypeArgs;
            if (MethodInfo.ReturnType == typeof(Task))
            {
                UnwrappedReturnType = typeof(void);
                IsAsync = true;
            }
            else if (MethodInfo.ReturnType.TryExtractTypeArguments(typeof(Task<>), out taskTypeArgs))
            {
                UnwrappedReturnType = taskTypeArgs[0];
                IsAsync = true;
            }
            else
            {
                UnwrappedReturnType = MethodInfo.ReturnType;
            }

            TypeMapper = typeMapper;
            this.parameters =
                new Lazy<IList<HandlerParameter>>(
                    () => methodInfo.GetParameters().Select(x => new HandlerParameter(x, this)).ToList());
            this.parameterTypes = new Lazy<IList<Type>>(() => Parameters.MapList(x => x.Type));
        }


        public bool IsAsync { get; }

        public MethodInfo MethodInfo { get; }

        public string Name => MethodInfo.Name;

        public IList<HandlerParameter> Parameters => this.parameters.Value;

        public IList<Type> ParameterTypes => this.parameterTypes.Value;

        public Type ReturnType => MethodInfo.ReturnType;

        public TypeMapper TypeMapper { get; }

        public Type UnwrappedReturnType { get; }


        public RouteAction Match(HttpMethod method, PathNodeType nodeType, TypeSpec resourceType)
        {
            switch (nodeType)
            {
                case PathNodeType.Collection:
                    return MatchCollectionNodeRequest(method, (ResourceType)resourceType);
                case PathNodeType.Resource:
                    if (!MethodInfo.Name.StartsWith(method.ToString()))
                        return null;
                    return MatchResourceNodeRequest(method, (ResourceType)resourceType);
            }
            return null;
        }


        public bool NameStartsWith(HttpMethod method)
        {
            return Name.StartsWith(method.ToString());
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
            if (!MethodInfo.Name.Equals("Get") && !MethodInfo.Name.Equals("Query") &&
                !MethodInfo.Name.Equals("Get" + resourceType.PluralName) &&
                !MethodInfo.Name.Equals("Query" + resourceType.PluralName))
                return null;

            // Check that the it takes a parameter of type Parent if the type is a child resource of Parent.
            if (resourceType.ParentResourceType != null)
            {
                var parentParameter = MethodInfo.GetParameters();
                if (parentParameter.Length != 1 ||
                    parentParameter[0].ParameterType != resourceType.ParentResourceType.Type)
                    return null;
            }

            // Check that it returns an IQueryable<Object>.
            if (!typeof(IQueryable<>).MakeGenericType(resourceType.Type).IsAssignableFrom(UnwrappedReturnType))
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
            if (UnwrappedReturnType != resourceType.Type)
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