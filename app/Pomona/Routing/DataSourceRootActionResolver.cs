#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class DataSourceRootActionResolver : IRouteActionResolver
    {
        public virtual IEnumerable<RouteAction> Resolve(Route route,
                                                        HttpMethod method)
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
                case HttpMethod.Post:
                    func = ResolvePost(route, resourceItemType);
                    break;
                case HttpMethod.Patch:
                    func = ResolvePatch(route, resourceItemType);
                    break;
            }
            if (func != null)
                yield return func;
        }


        private Func<PomonaRequest, PomonaResponse> ResolveGet(Route route, ResourceType resourceType)
        {
            if (route.ResultType.IsCollection)
                return ResolveGetCollection(route, resourceType);
            return null;
        }


        private Func<PomonaRequest, PomonaResponse> ResolveGetCollection(Route route, ResourceType resourceType)
        {
            var dataSourceCollectionRoute = route as DataSourceCollectionRoute;
            if (dataSourceCollectionRoute != null)
            {
                return pr =>
                {
                    Type[] qTypeArgs;
                    var elementType = resourceType.Type;
                    if (pr.AcceptType != null
                        && pr.AcceptType.TryExtractTypeArguments(typeof(IQueryable<>), out qTypeArgs))
                        elementType = qTypeArgs[0];

                    return new PomonaResponse(pr, pr.Session.GetInstance<IPomonaDataSource>().Query(elementType));
                };
            }
            return null;
        }


        private Func<PomonaRequest, PomonaResponse> ResolvePatch(Route route, ResourceType resourceItemType)
        {
            if (route.IsSingle)
            {
                return pr =>
                {
                    var patchedObject = pr.Bind();
                    return
                        new PomonaResponse(pr,
                                           pr.Session.GetInstance<IPomonaDataSource>().Patch(patchedObject.GetType(),
                                                                                             patchedObject));
                };
            }
            return null;
        }


        private Func<PomonaRequest, PomonaResponse> ResolvePost(Route route, ResourceType resourceItemType)
        {
            if (route.NodeType == PathNodeType.Collection)
                return ResolvePostToCollection(route, resourceItemType);
            return null;
        }


        private Func<PomonaRequest, PomonaResponse> ResolvePostToCollection(Route route, ResourceType resourceItemType)
        {
            if (route.ResultItemType is ResourceType && route.ResultType.IsCollection
                && route.Root() is DataSourceRootRoute)
            {
                return pr =>
                {
                    var form = pr.Bind(resourceItemType);
                    return new PomonaResponse(pr, pr.Session.GetInstance<IPomonaDataSource>().Post(form.GetType(), form));
                };
            }
            return null;
        }
    }
}