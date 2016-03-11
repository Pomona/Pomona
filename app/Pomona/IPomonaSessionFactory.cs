#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;
using Pomona.Routing;

namespace Pomona
{
    public interface IPomonaSessionFactory
    {
        IRouteActionResolver ActionResolver { get; }
        IRequestProcessorPipeline Pipeline { get; }
        Route Routes { get; }
        ITextSerializerFactory SerializerFactory { get; }
        TypeMapper TypeMapper { get; }
        IPomonaSession CreateSession(IContainer container);
    }
}