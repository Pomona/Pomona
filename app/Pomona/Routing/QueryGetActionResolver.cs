#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Common.Expressions;
using Pomona.Common.Internals;
using Pomona.Common.Linq.NonGeneric;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;

namespace Pomona.Routing
{
    public class QueryGetActionResolver : IRouteActionResolver
    {
        private readonly IQueryProviderCapabilityResolver capabilityResolver;


        public QueryGetActionResolver(IQueryProviderCapabilityResolver capabilityResolver)
        {
            if (capabilityResolver == null)
                throw new ArgumentNullException(nameof(capabilityResolver));
            this.capabilityResolver = capabilityResolver;
        }


        protected virtual Func<PomonaContext, Task<PomonaResponse>> ResolveGet(Route route, ResourceType resourceType)
        {
            if (route.ResultType.IsCollection)
                return ResolveGetCollection(route, resourceType);
            return ResolveGetSingle(route, resourceType);
        }


        protected virtual Func<PomonaContext, Task<PomonaResponse>> ResolveGetById(GetByIdRoute route,
                                                                             ResourceType resourceType)
        {
            var idProp = route.IdProperty;
            var idType = idProp.PropertyType;
            return
                async pr =>
                {
                    var segmentValue = pr.Node.PathSegment.Parse(idType);
                    return new PomonaResponse(pr,
                                              (await pr.Node.Parent.QueryAsync())
                                                .WhereEx(
                                                    ex =>
                                                        ex.Apply(idProp.CreateGetterExpression)
                                                        == Ex.Const(segmentValue, idType))
                                                .WrapActionResult(QueryProjection.FirstOrDefault));
                };
        }


        protected virtual Func<PomonaContext, Task<PomonaResponse>> ResolveGetCollection(Route route,
                                                                                   ResourceType resourceType)
        {
            var propertyRoute = route as ResourcePropertyRoute;
            if (propertyRoute != null)
                return ResolveGetCollectionProperty(propertyRoute, propertyRoute.Property, resourceType);
            return null;
        }


        protected virtual Func<PomonaContext, Task<PomonaResponse>> ResolveGetCollectionProperty(ResourcePropertyRoute route,
                                                                                           ResourceProperty property,
                                                                                           ResourceType resourceItemType)
        {
            if (this.capabilityResolver.PropertyIsMapped(property.PropertyInfo) || property.GetPropertyFormula() != null)
            {
                return
                    async pr =>
                    {
                        // Check existance of parent here, cannot differentiate between an empty collection and not found.
                        var parent = pr.Node.Parent;
                        if (parent.Route.IsSingle)
                        {
                            if (!await parent.ExistsAsync())
                                throw new ResourceNotFoundException("Resource not found.");
                        }

                        return new PomonaResponse(
                            (await parent.QueryAsync())
                                .OfTypeIfRequired(pr.Node.Route.InputType)
                                .SelectManyEx(x => x.Apply(property.CreateGetterExpression))
                                .WrapActionResult(defaultPageSize : property.ExposedAsRepository ? (int?)null : int.MaxValue));
                    };
            }
            else
            {
                return
                    async pr =>
                    {
                        var parentNode = pr.Node.Parent;
                        if (parentNode.Route.IsSingle)
                            return new PomonaResponse(((IEnumerable)property.GetValue(await parentNode.GetValueAsync())).AsQueryable());
                        return new PomonaResponse(
                            (await pr.Node.Parent.QueryAsync())
                              .OfTypeIfRequired(pr.Node.Route.InputType)
                              .ToListDetectType()
                              .AsQueryable()
                              .SelectManyEx(x => x.Apply(property.CreateGetterExpression))
                              .WrapActionResult(defaultPageSize : property.ExposedAsRepository ? (int?)null : int.MaxValue));
                    };
            }
        }


        protected virtual Func<PomonaContext, Task<PomonaResponse>> ResolveGetSingle(Route route, ResourceType resourceType)
        {
            var getByIdRoute = route as GetByIdRoute;
            if (getByIdRoute != null && !getByIdRoute.IsRoot)
                return ResolveGetById(getByIdRoute, resourceType);

            var propertyRoute = route as ResourcePropertyRoute;
            if (propertyRoute != null)
                return ResolveGetSingleProperty(propertyRoute, propertyRoute.Property, resourceType);

            return null;
        }


        protected virtual Func<PomonaContext, Task<PomonaResponse>> ResolveGetSingleProperty(ResourcePropertyRoute route,
                                                                                       StructuredProperty property,
                                                                                       ResourceType resourceType)
        {
            return
                async pr =>
                    new PomonaResponse(pr,
                                       (await pr.Node.Parent.QueryAsync())
                                         .SelectEx(x => x.Apply(property.CreateGetterExpression))
                                         .WrapActionResult(QueryProjection.FirstOrDefault));
        }


        public IEnumerable<RouteAction> Resolve(Route route, HttpMethod method)
        {
            var resourceItemType = route.ResultItemType as ResourceType;
            if (resourceItemType == null)
                yield break;

            RouteAction func = null;
            switch (method)
            {
                case HttpMethod.Get:
                    func = ResolveGet(route, resourceItemType);
                    break;
            }
            if (func != null)
                yield return func;
        }
    }
}