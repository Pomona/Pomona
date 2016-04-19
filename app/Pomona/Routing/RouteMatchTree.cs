#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;

namespace Pomona.Routing
{
    public class RouteMatchTree
    {
        public RouteMatchTree(Route route, string path, IPomonaSession session)
        {
            Session = session;
            Root = new UrlSegment(route,
                                  path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(HttpUtility.UrlDecode)
                                      .ToArray(), this);
        }


        public IEnumerable<UrlSegment> Leafs
        {
            get { return Root.FinalMatchCandidates; }
        }

        public int MatchCount
        {
            get { return Root.FinalMatchCandidates.Count(); }
        }

        public UrlSegment Root { get; }

        public IPomonaSession Session { get; }
    }
}