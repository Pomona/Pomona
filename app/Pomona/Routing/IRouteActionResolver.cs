#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Pomona.Common;

namespace Pomona.Routing
{
    public interface IRouteActionResolver
    {
        IEnumerable<RouteAction> Resolve(Route route, HttpMethod method);
    }
}
