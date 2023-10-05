#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona
{
    public interface IPomonaSession : IContainer
    {
        PomonaContext CurrentContext { get; }
        IPomonaSessionFactory Factory { get; }
        Route Routes { get; }
        TypeMapper TypeMapper { get; }
        PomonaResponse Dispatch(PomonaContext context);
        PomonaResponse Dispatch(PomonaRequest request);
        IEnumerable<RouteAction> GetRouteActions(PomonaContext context);
    }
}

