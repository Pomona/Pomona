#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;
using Pomona.Routing;

namespace Pomona
{
    internal class PomonaSessionFactory : IPomonaSessionFactory
    {
        private readonly DefaultRequestProcessorPipeline pipeline;
        private readonly PomonaJsonSerializerFactory serializerFactory;


        public PomonaSessionFactory(TypeMapper typeMapper, Route routes, IRouteActionResolver actionResolver)
        {
            TypeMapper = typeMapper;
            ActionResolver = actionResolver;
            Routes = routes;
            this.pipeline = new DefaultRequestProcessorPipeline();
            this.serializerFactory = new PomonaJsonSerializerFactory();
        }


        public IRouteActionResolver ActionResolver { get; }


        public IPomonaSession CreateSession(IContainer container, IUriResolver uriResolver)
        {
            return new PomonaSession(this, container, uriResolver);
        }


        public IRequestProcessorPipeline Pipeline
        {
            get { return this.pipeline; }
        }

        public Route Routes { get; }

        public ITextSerializerFactory SerializerFactory
        {
            get { return this.serializerFactory; }
        }

        public TypeMapper TypeMapper { get; }
    }
}