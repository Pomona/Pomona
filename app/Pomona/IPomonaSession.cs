#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona
{
    public interface IPomonaSession : IContainer
    {
        /// <summary>
        /// This should be removed at a later point.
        /// </summary>
        ITextDeserializer Deserializer { get; }

        Route Routes { get; }
        ISerializationContextProvider SerializationContextProvider { get; }
        TypeMapper TypeResolver { get; }
        IUriResolver UriResolver { get; }
        PomonaResponse Dispatch(PomonaContext context);
        PomonaResponse Dispatch(PomonaRequest request);
        IEnumerable<RouteAction> GetRouteActions(PomonaContext context);
    }
}