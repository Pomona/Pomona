using Microsoft.Practices.ServiceLocation;

namespace Pomona
{
    public class UriResolver
    {
        private PomonaModule pomonaModule;
        private readonly IServiceLocator container;

        public UriResolver(PomonaModule pomonaModule)
        {
            this.pomonaModule = pomonaModule;
        }

        public UriResolver(IServiceLocator container, PomonaModule pomonaModule)
        {
            this.container = container;
            this.pomonaModule = pomonaModule;
        }

        object IPomonaUriResolver.GetResultByUri(string uriString)
        {
            var routeResolver = container.GetInstance<IRouteResolver>();
            var uri = new Uri(uriString, UriKind.Absolute);

            var modulePath = uri.AbsolutePath;
            var basePath = pomonaModule.Request.Url.BasePath ?? string.Empty;
            if (modulePath.StartsWith(basePath))
                modulePath = modulePath.Substring(basePath.Length);

            var url = pomonaModule.Request.Url.Clone();
            url.Path = modulePath;
            url.Query = uri.Query;

            var innerRequest = new Request("GET", url);
            var innerContext = new NancyContext
                {
                    Culture = pomonaModule.Context.Culture,
                    CurrentUser = pomonaModule.Context.CurrentUser,
                    Request = innerRequest
                };

            var routeMatch = routeResolver.Resolve(innerContext);
            var route = routeMatch.Route;
            var dynamicDict = routeMatch.Parameters;

            var pomonaResponse = (PomonaResponse) route.Action((dynamic) dynamicDict);

            return pomonaResponse.Entity;
        }
    }
}