#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

using Pomona.Common;
using Pomona.Common.Internals;

namespace Pomona.Routing
{
    internal class InternalRouteActionResolver : IRouteActionResolver
    {
        private readonly IEnumerable<IRouteActionResolver> nestedActionResolvers;

        private readonly ReadOnlyDictionary<HttpMethod, ConcurrentDictionary<Route, IEnumerable<RouteAction>>>
            routeActionCache;


        public InternalRouteActionResolver(IEnumerable<IRouteActionResolver> nestedActionResolvers)
        {
            if (nestedActionResolvers == null)
                throw new ArgumentNullException(nameof(nestedActionResolvers));
            this.nestedActionResolvers = nestedActionResolvers.ToList();
            this.routeActionCache =
                new ReadOnlyDictionary<HttpMethod, ConcurrentDictionary<Route, IEnumerable<RouteAction>>>(
                    Enum.GetValues(typeof(HttpMethod))
                        .Cast<HttpMethod>()
                        .ToDictionary(x => x, x => new ConcurrentDictionary<Route, IEnumerable<RouteAction>>()));
        }


        private IEnumerable<RouteAction> ResolveNonCached(Route route, HttpMethod method)
        {
            // First check whether there's a matching handler for type
            if (!route.AllowedMethods.HasFlag(method))
            {
                return
                    RouteAction.Create(pr => ThrowMethodNotAllowedForType(method, route.AllowedMethods)).WrapAsArray();
            }

            var routeActions = this.nestedActionResolvers
                                   .SelectMany(x => x.Resolve(route, method).EmptyIfNull()).ToList();
            if (routeActions.Count > 0)
                return routeActions;

            return RouteAction.Create(x =>
            {
                throw new PomonaServerException("Unable to resolve action for route.",
                                                null,
                                                statusCode : HttpStatusCode.Forbidden);
            }).WrapAsArray();
        }


        private static PomonaResponse ThrowMethodNotAllowedForType(HttpMethod requestMethod, HttpMethod allowedMethods)
        {
            var httpMethods = Enum.GetValues(typeof(HttpMethod))
                                  .Cast<HttpMethod>()
                                  .Where(x => allowedMethods.HasFlag(x))
                                  .Select(x => x.ToString().ToUpper());

            var allowedMethodsString = String.Join(", ", httpMethods);

            var allowHeader = new KeyValuePair<string, string>("Allow", allowedMethodsString);

            throw new PomonaServerException(
                $"Method {requestMethod.ToString().ToUpper()} not allowed!",
                null,
                HttpStatusCode.MethodNotAllowed,
                allowHeader.WrapAsEnumerable());
        }


        public IEnumerable<RouteAction> Resolve(Route route, HttpMethod method)
        {
            return this.routeActionCache[method].GetOrAdd(route, r => ResolveNonCached(r, method));
        }
    }
}