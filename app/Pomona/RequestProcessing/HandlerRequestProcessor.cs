#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public abstract class HandlerRequestProcessor : IRouteActionResolver
    {
        public static HandlerRequestProcessor Create(Type type)
        {
            return
                (HandlerRequestProcessor)
                    Activator.CreateInstance(typeof(HandlerRequestProcessor<>).MakeGenericType(type));
        }


        public abstract IEnumerable<RouteAction> Resolve(Route route, HttpMethod method);
    }

    public class HandlerRequestProcessor<THandler> : HandlerRequestProcessor
    {
        public IEnumerable<HandlerMethod> GetHandlerMethods(TypeMapper mapper)
        {
            return typeof(THandler).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                   .Select(x => new HandlerMethod(x, mapper));
        }


        public override IEnumerable<RouteAction> Resolve(Route route, HttpMethod method)
        {
            var handlerMethodInvokers = ResolveHandlerMethods(route, method);
            return handlerMethodInvokers;
        }


        private IEnumerable<RouteAction> ResolveHandlerMethods(Route route, HttpMethod method)
        {
            var resourceType = route.ResultItemType;
            var typeSpec = resourceType;
            return
                GetHandlerMethods((TypeMapper)typeSpec.TypeResolver).Select(
                    x => x.Match(method, route.NodeType, typeSpec)).Where(x => x != null).ToList();
        }
    }
}