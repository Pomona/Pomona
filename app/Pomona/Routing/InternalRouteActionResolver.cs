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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Nancy;

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
                throw new ArgumentNullException("nestedActionResolvers");
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
                string.Format("Method {0} not allowed!", requestMethod.ToString().ToUpper()),
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