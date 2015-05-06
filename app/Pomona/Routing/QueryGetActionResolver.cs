#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                throw new ArgumentNullException("capabilityResolver");
            this.capabilityResolver = capabilityResolver;
        }


        protected virtual Func<PomonaContext, PomonaResponse> ResolveGet(Route route, ResourceType resourceType)
        {
            if (route.ResultType.IsCollection)
                return ResolveGetCollection(route, resourceType);
            return ResolveGetSingle(route, resourceType);
        }


        protected virtual Func<PomonaContext, PomonaResponse> ResolveGetById(GetByIdRoute route,
                                                                             ResourceType resourceType)
        {
            var idProp = route.IdProperty;
            var idType = idProp.PropertyType;
            return
                pr =>
                {
                    var segmentValue = pr.Node.PathSegment.Parse(idType);
                    return new PomonaResponse(pr,
                                              pr.Node.Parent
                                                .Query()
                                                .WhereEx(
                                                    ex =>
                                                        ex.Apply(idProp.CreateGetterExpression)
                                                        == Ex.Const(segmentValue, idType))
                                                .WrapActionResult(QueryProjection.FirstOrDefault));
                };
        }


        protected virtual Func<PomonaContext, PomonaResponse> ResolveGetCollection(Route route,
                                                                                   ResourceType resourceType)
        {
            var propertyRoute = route as ResourcePropertyRoute;
            if (propertyRoute != null)
                return ResolveGetCollectionProperty(propertyRoute, propertyRoute.Property, resourceType);
            return null;
        }


        protected virtual Func<PomonaContext, PomonaResponse> ResolveGetCollectionProperty(ResourcePropertyRoute route,
                                                                                           ResourceProperty property,
                                                                                           ResourceType resourceItemType)
        {
            if (this.capabilityResolver.PropertyIsMapped(property.PropertyInfo) || property.GetPropertyFormula() != null)
            {
                return
                    pr =>
                    {
                        // Check existance of parent here, cannot differentiate between an empty collection and not found.
                        var parent = pr.Node.Parent;
                        if (parent.Route.IsSingle)
                        {
                            if (!parent.Exists)
                                throw new ResourceNotFoundException("Resource not found.");
                        }

                        return new PomonaResponse(
                            parent
                                .Query()
                                .OfTypeIfRequired(pr.Node.Route.InputType)
                                .SelectManyEx(x => x.Apply(property.CreateGetterExpression))
                                .WrapActionResult(defaultPageSize : property.ExposedAsRepository ? (int?)null : int.MaxValue));
                    };
            }
            else
            {
                return
                    pr =>
                    {
                        var parentNode = pr.Node.Parent;
                        if (parentNode.Route.IsSingle)
                            return new PomonaResponse(((IEnumerable)property.GetValue(parentNode.Get())).AsQueryable());
                        return new PomonaResponse(
                            pr.Node.Parent
                              .Query()
                              .OfTypeIfRequired(pr.Node.Route.InputType)
                              .ToListDetectType()
                              .AsQueryable()
                              .SelectManyEx(x => x.Apply(property.CreateGetterExpression))
                              .WrapActionResult(defaultPageSize : property.ExposedAsRepository ? (int?)null : int.MaxValue));
                    };
            }
        }


        protected virtual Func<PomonaContext, PomonaResponse> ResolveGetSingle(Route route, ResourceType resourceType)
        {
            var getByIdRoute = route as GetByIdRoute;
            if (getByIdRoute != null && !getByIdRoute.IsRoot)
                return ResolveGetById(getByIdRoute, resourceType);

            var propertyRoute = route as ResourcePropertyRoute;
            if (propertyRoute != null)
                return ResolveGetSingleProperty(propertyRoute, propertyRoute.Property, resourceType);

            return null;
        }


        protected virtual Func<PomonaContext, PomonaResponse> ResolveGetSingleProperty(ResourcePropertyRoute route,
                                                                                       StructuredProperty property,
                                                                                       ResourceType resourceType)
        {
            return
                pr =>
                    new PomonaResponse(pr,
                                       pr.Node.Parent.Query()
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