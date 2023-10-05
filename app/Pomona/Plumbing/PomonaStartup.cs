#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Nancy.Bootstrapper;
using Nancy.Routing;

namespace Pomona.Plumbing
{
    public class PomonaStartup : IApplicationStartup
    {
        private readonly IRouteResolver routeResolver;


        public PomonaStartup(IRouteResolver routeResolver)
        {
            this.routeResolver = routeResolver;
        }


        public void Initialize(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(context =>
            {
                context.Items[typeof(IRouteResolver).FullName] = this.routeResolver;
                return null;
            });
        }
    }
}

