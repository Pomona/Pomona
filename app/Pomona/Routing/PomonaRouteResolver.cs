#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

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


        public RouteMatchTree Resolve(IPomonaSession session, string path)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var match = new RouteMatchTree(this.rootRoute, path, session);

            return match.MatchCount > 0 ? match : null;
        }
    }
}