#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;

namespace Pomona.Routing
{
    public class RequestHandlerActionResolver : IRouteActionResolver
    {
        public IEnumerable<RouteAction> Resolve(Route route, HttpMethod method)
        {
            // First check whether there's a matching handler for type
            var resourceItemType = route.ResultItemType as ResourceType;
            if (resourceItemType == null)
                return Enumerable.Empty<RouteAction>();

            var routeActions =
                resourceItemType
                    .WalkTree(x => x.ParentResourceType)
                    .SelectMany(y => y.ResourceHandlers)
                    .Select(HandlerRequestProcessor.Create)
                    .SelectMany(x => x.Resolve(route, method));
            return routeActions;
        }
    }
}