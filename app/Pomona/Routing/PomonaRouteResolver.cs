#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;

using Pomona.Common.Internals;

namespace Pomona.Routing
{
    internal class PomonaRouteResolver
    {
        private readonly Route rootRoute;


        public PomonaRouteResolver(Route rootRoute)
        {
            if (rootRoute == null)
                throw new ArgumentNullException(nameof(rootRoute));
            if (!rootRoute.IsRoot)
                throw new ArgumentException("The route resolver takes a root route.");
            this.rootRoute = rootRoute;
        }


        public RouteMatchTree GetMatch(IPomonaSession session, string path)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var match = new RouteMatchTree(this.rootRoute, path, session);

            return match.MatchCount > 0 ? match : null;
        }


        public async Task<UrlSegment> Resolve(IPomonaSession session, PomonaRequest request)
        {
            var match = GetMatch(session, request.RelativePath);

            if (match == null)
                throw new ResourceNotFoundException("Resource not found.");

            var finalSegmentMatch = match.Root.SelectedFinalMatch;
            if (finalSegmentMatch == null)
            {
                // Route conflict resolution:
                var node = match.Root.NextConflict;
                while (node != null)
                {
                    var actualResultType = await node.GetActualResultTypeAsync();
                    // Reduce using input type difference
                    var validSelection =
                        node.Children.Where(x => x.Route.InputType.IsAssignableFrom(actualResultType))
                            .SingleOrDefaultIfMultiple();
                    if (validSelection == null)
                        throw new ResourceNotFoundException("No route alternative found due to conflict.");
                    node.SelectedChild = validSelection;
                    node = node.NextConflict;
                }
                finalSegmentMatch = match.Root.SelectedFinalMatch;
            }
            return finalSegmentMatch;
        }
    }
}