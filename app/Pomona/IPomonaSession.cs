#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Threading.Tasks;

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

        ISerializationContextProvider SerializationContextProvider { get; }
        TypeMapper TypeResolver { get; }
        IUriResolver UriResolver { get; }
        Task<PomonaResponse> Dispatch(PomonaContext context);
        Task<PomonaResponse> Dispatch(PomonaRequest request);
        IEnumerable<RouteAction> GetRouteActions(PomonaContext context);
    }
}