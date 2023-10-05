#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.Loading;
using Pomona.Common.Proxies;
using Pomona.Common.Web;

namespace Pomona.Common.Linq
{
    using ExecuteWithClientSelectPartDelegate =
        Func<Type, Type, RestQueryProvider, string, RestQueryableTreeParser.QueryProjection, LambdaExpression,
            RequestOptions, bool, object>;
    using ExecuteGenericMethodDelegate = Func<Type, RestQueryProvider, RestQueryableTreeParser, bool, object>;

    public class RestQueryProvider : QueryProviderBase
    {
        private static readonly ExecuteGenericMethodDelegate executeGenericMethod;
        private static readonly ExecuteWithClientSelectPartDelegate executeWithClientSelectPart;


        static RestQueryProvider()
        {
            executeGenericMethod = GenericInvoker
                .Instance<RestQueryProvider>()
                .CreateFunc1<RestQueryableTreeParser, bool, object>(x => x.Execute<object>(null, false));

            executeWithClientSelectPart = GenericInvoker
                .Instance<RestQueryProvider>()
                .CreateFunc2<string, RestQueryableTreeParser.QueryProjection, LambdaExpression, RequestOptions, bool, object>(
                    x => x.ExecuteWithClientSelectPart<int, bool>(null,
                                                                  default(RestQueryableTreeParser.QueryProjection),
                                                                  null,
                                                                  null,
                                                                  false));
        }


        internal RestQueryProvider(IPomonaClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;
        }


        internal IPomonaClient Client { get; }


        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new RestQuery<TElement>(this, expression);
        }


        public IQueryable<T> CreateQuery<T>(string uri)
        {
            return new RestQueryRoot<T>(this, uri);
        }


        public IQueryable CreateQuery(string uri, Type type)
        {
            return (IQueryable)Activator.CreateInstance(typeof(RestQueryRoot<>).MakeGenericType(type), this, uri);
        }


        public override object Execute(Expression expression, Type resultType)
        {
            var queryTreeParser = new RestQueryableTreeParser();
            queryTreeParser.Visit(expression);

            var executeAsync = resultType != null && typeof(Task).IsAssignableFrom(resultType);

            return executeGenericMethod.Invoke(queryTreeParser.SelectReturnType, this, queryTreeParser, executeAsync);
        }


        public string GetQueryText(Expression expression)
        {
            return expression.ToString();
        }


        private string BuildUri(RestQueryableTreeParser parser, out LambdaExpression clientSideSelectPart)
        {
            clientSideSelectPart = null;
            var builder = new UriQueryBuilder();

            var resourceInfo = Client.GetResourceInfoForType(parser.ElementType);

            if (!resourceInfo.IsUriBaseType)
                builder.AppendParameter("$oftype", resourceInfo.JsonTypeName);

            SetProjection(parser, builder);

            if (parser.WherePredicate != null)
                builder.AppendExpressionParameter("$filter", parser.WherePredicate);
            if (parser.OrderKeySelectors.Count > 0)
                builder.AppendExpressionParameter<QueryOrderByBuilder>("$orderby", Expression.Constant(parser.OrderKeySelectors));
            if (parser.GroupByKeySelector != null)
                builder.AppendExpressionParameter<QuerySelectorBuilder>("$groupby", parser.GroupByKeySelector);
            if (parser.SelectExpression != null)
            {
                var selectNode = parser.SelectExpression.Visit<ClientServerSplittingSelectBuilder>();
                var splitSelect = selectNode as ClientServerSplitSelectExpression;
                if (splitSelect != null)
                {
                    clientSideSelectPart = splitSelect.ClientSideExpression;
                    selectNode = splitSelect.ServerExpression;
                }
                builder.AppendParameter("$select", selectNode);
            }

            if (parser.Projection == RestQueryableTreeParser.QueryProjection.Enumerable)
            {
                if (parser.SkipCount.HasValue)
                    builder.AppendParameter("$skip", parser.SkipCount.Value);
                builder.AppendParameter("$top", parser.TakeCount.GetValueOrDefault(int.MaxValue));
            }

            var expandedPaths = parser.ExpandedPaths;
            if (!string.IsNullOrEmpty(expandedPaths))
                builder.AppendParameter("$expand", expandedPaths);

            if (parser.IncludeTotalCount)
                builder.AppendParameter("$totalcount", "true");

            return parser.RepositoryUri + "?" + builder;
        }


        private object Execute<T>(RestQueryableTreeParser parser, bool executeAsync)
        {
            LambdaExpression clientSideSelectPart;
            var uri = BuildUri(parser, out clientSideSelectPart);

            if (parser.ResultMode == RestQueryableTreeParser.ResultModeType.ToUri)
                return new Uri(uri);

            var requestOptions = RequestOptions.Create<T>(x => parser.RequestOptionActions.ForEach(y => y(x)));
            if (parser.ResultMode == RestQueryableTreeParser.ResultModeType.ToJson)
                return Client.Get<JToken>(uri, requestOptions);

            var queryProjection = parser.Projection;

            if (clientSideSelectPart != null)
            {
                return executeWithClientSelectPart.Invoke(clientSideSelectPart.Parameters[0].Type,
                                                          clientSideSelectPart.ReturnType,
                                                          this,
                                                          uri,
                                                          queryProjection,
                                                          clientSideSelectPart,
                                                          requestOptions,
                                                          executeAsync);
            }

            return Execute<T, T>(uri, queryProjection, null, requestOptions, executeAsync);
        }


        private object Execute<T, TConverted>(string uri,
                                              RestQueryableTreeParser.QueryProjection queryProjection,
                                              Func<T, TConverted> clientSideSelectPart,
                                              RequestOptions requestOptions,
                                              bool executeAsync)
        {
            if (executeAsync)
                return ExecuteAsync(uri, queryProjection, clientSideSelectPart, requestOptions);

            ;
            return ExecuteSync(uri, queryProjection, clientSideSelectPart, requestOptions);
        }


        private object ExecuteAsync<T, TConverted>(string uri,
                                                   RestQueryableTreeParser.QueryProjection queryProjection,
                                                   Func<T, TConverted> clientSideSelectPart,
                                                   RequestOptions requestOptions)
        {
            if (queryProjection == RestQueryableTreeParser.QueryProjection.FirstLazy)
                throw new InvalidOperationException("Don't use FirstLazy when executing async as this is redundant.");

            switch (queryProjection)
            {
                case RestQueryableTreeParser.QueryProjection.ToQueryResult:
                    return ExecuteToQueryResultAsync(uri, clientSideSelectPart, requestOptions);

                case RestQueryableTreeParser.QueryProjection.Enumerable:
                    return ExecuteToEnumerableAsync(uri, clientSideSelectPart, requestOptions);
                case RestQueryableTreeParser.QueryProjection.SingleOrDefault:
                case RestQueryableTreeParser.QueryProjection.First:
                case RestQueryableTreeParser.QueryProjection.FirstOrDefault:
                case RestQueryableTreeParser.QueryProjection.Single:
                case RestQueryableTreeParser.QueryProjection.Last:
                case RestQueryableTreeParser.QueryProjection.LastOrDefault:
                    return GetOneAsync(uri, requestOptions, clientSideSelectPart);
                case RestQueryableTreeParser.QueryProjection.Any:
                case RestQueryableTreeParser.QueryProjection.Max:
                case RestQueryableTreeParser.QueryProjection.Min:
                case RestQueryableTreeParser.QueryProjection.Sum:
                case RestQueryableTreeParser.QueryProjection.Count:
                    return Client.GetAsync<TConverted>(uri, requestOptions);
                default:
                    throw new NotImplementedException("Don't recognize projection type " + queryProjection);
            }
        }


        private object ExecuteSync<T, TConverted>(string uri,
                                                  RestQueryableTreeParser.QueryProjection queryProjection,
                                                  Func<T, TConverted> clientSideSelectPart,
                                                  RequestOptions requestOptions)
        {
            if (queryProjection == RestQueryableTreeParser.QueryProjection.FirstLazy)
            {
                var resourceLoader = requestOptions == null || requestOptions.ResourceLoader == null
                    ? new DefaultResourceLoader(Client)
                    : requestOptions.ResourceLoader;

                var resourceInfo = Client.GetResourceInfoForType(typeof(T));
                var proxy = (LazyProxyBase)Activator.CreateInstance(resourceInfo.LazyProxyType);
                proxy.Initialize(uri, resourceLoader, resourceInfo.PocoType);
                return proxy;
            }

            switch (queryProjection)
            {
                case RestQueryableTreeParser.QueryProjection.ToQueryResult:
                {
                    var result = Client.Get<QueryResult<T>>(uri, requestOptions);
                    if (clientSideSelectPart != null)
                    {
                        return new QueryResult<TConverted>(result.Select(clientSideSelectPart), result.Skip, result.TotalCount,
                                                           result.Previous,
                                                           result.Next);
                    }
                    return result;
                }

                case RestQueryableTreeParser.QueryProjection.Enumerable:
                {
                    var result = Client.Get<IList<T>>(uri, requestOptions);
                    if (clientSideSelectPart != null)
                        return result.Select(clientSideSelectPart).ToList();
                    return result;
                }
                case RestQueryableTreeParser.QueryProjection.SingleOrDefault:
                case RestQueryableTreeParser.QueryProjection.First:
                case RestQueryableTreeParser.QueryProjection.FirstOrDefault:
                case RestQueryableTreeParser.QueryProjection.Single:
                case RestQueryableTreeParser.QueryProjection.Last:
                case RestQueryableTreeParser.QueryProjection.LastOrDefault:
                    return GetOne(uri, requestOptions, clientSideSelectPart);
                case RestQueryableTreeParser.QueryProjection.Any:
                    // TODO: Remove backwards compatibility at some point in the future.
                    // HACK for backwards compatibility. Any is not supported by old versions of pomona server
                    return Client.Get<int>(uri, requestOptions) > 0;
                case RestQueryableTreeParser.QueryProjection.Max:
                case RestQueryableTreeParser.QueryProjection.Min:
                case RestQueryableTreeParser.QueryProjection.Sum:
                case RestQueryableTreeParser.QueryProjection.Count:
                    return Client.Get<T>(uri, requestOptions);
                default:
                    throw new NotImplementedException("Don't recognize projection type " + queryProjection);
            }
        }


        private async Task<IEnumerable<TConverted>> ExecuteToEnumerableAsync<T, TConverted>(string uri,
                                                                                            Func<T, TConverted> clientSideSelectPart,
                                                                                            RequestOptions requestOptions)
        {
            {
                var result = await Client.GetAsync<IEnumerable<T>>(uri, requestOptions);
                if (clientSideSelectPart != null)
                    return result.Select(clientSideSelectPart).ToList();
                return (QueryResult<TConverted>)((object)result);
            }
        }


        private async Task<QueryResult<TConverted>> ExecuteToQueryResultAsync<T, TConverted>(string uri,
                                                                                             Func<T, TConverted> clientSideSelectPart,
                                                                                             RequestOptions requestOptions)
        {
            {
                var result = await Client.GetAsync<QueryResult<T>>(uri, requestOptions);
                if (clientSideSelectPart != null)
                {
                    return new QueryResult<TConverted>(result.Select(clientSideSelectPart), result.Skip, result.TotalCount, result.Previous,
                                                       result.Next);
                }
                return (QueryResult<TConverted>)((object)result);
            }
        }


        private object ExecuteWithClientSelectPart<TServer, TClient>(string uri,
                                                                     RestQueryableTreeParser.QueryProjection
                                                                         queryProjection,
                                                                     Expression<Func<TServer, TClient>>
                                                                         clientSideExpression,
                                                                     RequestOptions requestOptions,
                                                                     bool executeAsync)
        {
            return Execute(uri, queryProjection, clientSideExpression.Compile(), requestOptions, executeAsync);
        }


        private object GetOne<T, TConverted>(string uri, RequestOptions requestOptions, Func<T, TConverted> clientSideSelectPart)
        {
            try
            {
                var result = Client.Get<T>(uri, requestOptions);
                if (clientSideSelectPart != null)
                    return clientSideSelectPart(result);
                return result;
            }
            catch (ResourceNotFoundException ex)
            {
                throw new InvalidOperationException("Sequence contains no matching element", ex);
            }
        }


        private async Task<TConverted> GetOneAsync<T, TConverted>(string uri,
                                                                  RequestOptions requestOptions,
                                                                  Func<T, TConverted> clientSideSelectPart)
        {
            try
            {
                var result = await Client.GetAsync<T>(uri, requestOptions);
                if (clientSideSelectPart != null)
                    return clientSideSelectPart(result);
                return (TConverted)((object)result);
            }
            catch (ResourceNotFoundException ex)
            {
                throw new InvalidOperationException("Sequence contains no matching element", ex);
            }
        }


        private static void SetProjection(RestQueryableTreeParser parser, UriQueryBuilder builder)
        {
            string projection = null;
            switch (parser.Projection)
            {
                case RestQueryableTreeParser.QueryProjection.First:
                case RestQueryableTreeParser.QueryProjection.FirstLazy:
                    projection = "first";
                    break;
                case RestQueryableTreeParser.QueryProjection.FirstOrDefault:
                    projection = "firstordefault";
                    break;
                case RestQueryableTreeParser.QueryProjection.Single:
                    projection = "single";
                    break;
                case RestQueryableTreeParser.QueryProjection.SingleOrDefault:
                    projection = "singleordefault";
                    break;
                case RestQueryableTreeParser.QueryProjection.Last:
                    projection = "last";
                    break;
                case RestQueryableTreeParser.QueryProjection.LastOrDefault:
                    projection = "lastordefault";
                    break;
                case RestQueryableTreeParser.QueryProjection.Max:
                    projection = "max";
                    break;
                case RestQueryableTreeParser.QueryProjection.Min:
                    projection = "min";
                    break;
                //case RestQueryableTreeParser.QueryProjection.Any:
                // TODO: Remove backwards compatibility at some point in the future.
                // HACK for backwards compatibility. Any is not supported by old versions of pomona server
                //    projection = "any";
                //    break;
                case RestQueryableTreeParser.QueryProjection.Any:
                case RestQueryableTreeParser.QueryProjection.Count:
                    projection = "count";
                    break;
                case RestQueryableTreeParser.QueryProjection.Sum:
                    projection = "sum";
                    break;
            }
            if (projection != null)
                builder.AppendParameter("$projection", projection);
        }
    }
}
