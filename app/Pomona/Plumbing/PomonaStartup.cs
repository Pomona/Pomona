using Microsoft.Practices.ServiceLocation;

using Nancy.Bootstrapper;
using Nancy.Routing;

using Pomona.Common.Serialization;

namespace Pomona.Plumbing
{
    public class PomonaStartup : IApplicationStartup
    {
        private readonly TypeMapper typeMapper;
        private readonly IServiceLocator serviceLocator;
        private readonly IRouteResolver routeResolver;


        public PomonaStartup(TypeMapper typeMapper, IRouteResolver routeResolver)
        {
            this.typeMapper = typeMapper;
            this.routeResolver = routeResolver;
        }


        public void Initialize(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(context =>
            {
                context.Items[typeof(ISerializationContextProvider).FullName] =
                    new ServerSerializationContextProvider(new UriResolver(this.typeMapper, new BaseUriResolver(context)),
                        new ResourceResolver(this.typeMapper, context, this.routeResolver),
                        context);
                return null;
            });
        }
    }
}