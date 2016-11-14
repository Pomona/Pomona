#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class DataSourceRouteActionResolver : IRouteActionResolver
    {
        private static IPomonaDataSource GetDataSource(IPomonaSession session)
        {
            var dataSourceType = session.Routes
                .MaybeAs<DataSourceRootRoute>()
                .Select(x => x.DataSource)
                .OrDefault(typeof(IPomonaDataSource));
            var dataSource = (IPomonaDataSource)session.GetInstance(dataSourceType);
            return dataSource;
        }


        private static Func<PomonaContext, PomonaResponse> ResolveGet(Route route, ResourceType resourceType)
        {
            if (route.ResultType.IsCollection)
                return ResolveGetCollection(route, resourceType);
            return null;
        }


        private static Func<PomonaContext, PomonaResponse> ResolveGetCollection(Route route, ResourceType resourceType)
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

                    var dataSource = GetDataSource(pr.Session);
                    var entity = dataSource.Query(elementType);
                    return new PomonaResponse(pr, entity);
                };
            }
            return null;
        }


        private Func<PomonaContext, PomonaResponse> ResolveGetRootResource(DataSourceRootRoute route)
        {
            return pr =>
            {
                var request = pr;
                var uriResolver = request.Session.GetInstance<IUriResolver>();
                var dictionary = route.Children.OfType<ILiteralRoute>()
                                      .ToDictionary(x => x.MatchValue, x => uriResolver.RelativeToAbsoluteUri(x.MatchValue));
                var repos = new SortedDictionary<string, string>(dictionary);
                var resultType = request.TypeMapper.FromType<IDictionary<string, string>>();
                return new PomonaResponse(repos, resultType : resultType);
            };
        }


        private static Func<PomonaContext, PomonaResponse> ResolvePatch(Route route, ResourceType resourceItemType)
        {
            if (route.IsSingle)
            {
                return pr =>
                {
                    var patchedObject = pr.Bind();
                    var dataSource = GetDataSource(pr.Session);
                    var patchedType = patchedObject.GetType();
                    return new PomonaResponse(pr, dataSource.Patch(patchedType, patchedObject));
                };
            }
            return null;
        }


        private static Func<PomonaContext, PomonaResponse> ResolvePost(Route route, ResourceType resourceItemType)
        {
            if (route.NodeType == PathNodeType.Collection)
                return ResolvePostToCollection(route, resourceItemType);
            return null;
        }


        private static Func<PomonaContext, PomonaResponse> ResolvePostToCollection(Route route, ResourceType resourceItemType)
        {
            if (route.ResultItemType is ResourceType
                && route.ResultType.IsCollection
                && route.Root() is DataSourceRootRoute)
            {
                return pr =>
                {
                    var form = pr.Bind(resourceItemType);
                    var dataSource = GetDataSource(pr.Session);
                    var entity = dataSource.Post(form.GetType(), form);
                    return new PomonaResponse(pr, entity);
                };
            }
            return null;
        }


        public virtual IEnumerable<RouteAction> Resolve(Route route, HttpMethod method)
        {
            var rootRoute = route as DataSourceRootRoute;
            if (rootRoute != null)
                yield return ResolveGetRootResource(rootRoute);

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
    }
}