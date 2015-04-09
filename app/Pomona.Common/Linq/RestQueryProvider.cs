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
using System.Linq.Expressions;

using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.Loading;
using Pomona.Common.Proxies;
using Pomona.Common.Web;

namespace Pomona.Common.Linq
{
    using ExecuteWithClientSelectPartDelegate =
        Func<Type, Type, RestQueryProvider, string, RestQueryableTreeParser.QueryProjection, LambdaExpression,
            RequestOptions, object>;
    using ExecuteGenericMethodDelegate = Func<Type, RestQueryProvider, RestQueryableTreeParser, object>;

    public class RestQueryProvider : QueryProviderBase
    {
        private static readonly ExecuteGenericMethodDelegate executeGenericMethod;
        private static readonly ExecuteWithClientSelectPartDelegate executeWithClientSelectPart;

        private readonly IPomonaClient client;


        static RestQueryProvider()
        {
            executeGenericMethod = GenericInvoker
                .Instance<RestQueryProvider>()
                .CreateFunc1<RestQueryableTreeParser, object>(x => x.Execute<object>(null));

            executeWithClientSelectPart = GenericInvoker
                .Instance<RestQueryProvider>()
                .CreateFunc2<string, RestQueryableTreeParser.QueryProjection, LambdaExpression, RequestOptions, object>(
                    x => x.ExecuteWithClientSelectPart<int, bool>(null,
                                                                  default(RestQueryableTreeParser.QueryProjection),
                                                                  null,
                                                                  null));
        }


        internal RestQueryProvider(IPomonaClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            this.client = client;
        }


        internal IPomonaClient Client
        {
            get { return this.client; }
        }


        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new RestQuery<TElement>(this, expression);
        }


        public override object Execute(Expression expression, Type resultType)
        {
            var queryTreeParser = new RestQueryableTreeParser();
            queryTreeParser.Visit(expression);

            return executeGenericMethod.Invoke(queryTreeParser.SelectReturnType, this, queryTreeParser);
        }


        public IQueryable<T> CreateQuery<T>(string uri)
        {
            return new RestQueryRoot<T>(this, uri);
        }


        public IQueryable CreateQuery(string uri, Type type)
        {
            return (IQueryable)Activator.CreateInstance(typeof(RestQueryRoot<>).MakeGenericType(type), this, uri);
        }


        public string GetQueryText(Expression expression)
        {
            return expression.ToString();
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
                case RestQueryableTreeParser.QueryProjection.Max:
                    projection = "max";
                    break;
                case RestQueryableTreeParser.QueryProjection.Min:
                    projection = "min";
                    break;
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


        private string BuildUri(RestQueryableTreeParser parser, out LambdaExpression clientSideSelectPart)
        {
            clientSideSelectPart = null;
            var builder = new UriQueryBuilder();

            var resourceInfo = this.client.GetResourceInfoForType(parser.ElementType);

            if (!resourceInfo.IsUriBaseType)
                builder.AppendParameter("$oftype", resourceInfo.JsonTypeName);

            SetProjection(parser, builder);

            if (parser.WherePredicate != null)
                builder.AppendExpressionParameter("$filter", parser.WherePredicate);
            if (parser.OrderKeySelectors.Count > 0)
            {
                var orderExpressions =
                    string.Join(",", parser.OrderKeySelectors.Select(
                        x =>
                            x.Item1.Visit<QueryPredicateBuilder>().ToString()
                            + (x.Item2 == SortOrder.Descending ? " desc" : string.Empty)));
                builder.AppendParameter("$orderby", orderExpressions);
            }
            if (parser.GroupByKeySelector != null)
            {
                var selectBuilder = parser.GroupByKeySelector.Visit<QuerySelectorBuilder>();
                builder.AppendParameter("$groupby", selectBuilder);
            }
            if (parser.SelectExpression != null)
            {
                var selectNode = parser.SelectExpression.Visit<ClientSideSplittingSelectBuilder>();
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


        private object Execute<T>(RestQueryableTreeParser parser)
        {
            LambdaExpression clientSideSelectPart;
            var uri = BuildUri(parser, out clientSideSelectPart);

            if (parser.ResultMode == RestQueryableTreeParser.ResultModeType.ToUri)
                return new Uri(uri);

            var requestOptions = RequestOptions.Create<T>(x => parser.RequestOptionActions.ForEach(y => y(x)));
            if (parser.ResultMode == RestQueryableTreeParser.ResultModeType.ToJson)
                return this.client.Get<JToken>(uri, requestOptions);

            var queryProjection = parser.Projection;

            if (clientSideSelectPart != null)
            {
                return executeWithClientSelectPart.Invoke(clientSideSelectPart.Parameters[0].Type,
                                                          clientSideSelectPart.ReturnType,
                                                          this,
                                                          uri,
                                                          queryProjection,
                                                          clientSideSelectPart,
                                                          requestOptions);
            }

            return Execute<T, T>(uri, queryProjection, null, requestOptions);
        }


        private object Execute<T, TConverted>(string uri,
                                              RestQueryableTreeParser.QueryProjection queryProjection,
                                              Func<T, TConverted> clientSideSelectPart,
                                              RequestOptions requestOptions)
        {
            if (queryProjection == RestQueryableTreeParser.QueryProjection.FirstLazy)
            {
                var resourceLoader = requestOptions == null || requestOptions.ResourceLoader == null
                    ? new DefaultResourceLoader(this.client)
                    : requestOptions.ResourceLoader;

                var resourceInfo = this.client.GetResourceInfoForType(typeof(T));
                var proxy = (LazyProxyBase)Activator.CreateInstance(resourceInfo.LazyProxyType);
                proxy.Initialize(uri, resourceLoader, resourceInfo.PocoType);
                return proxy;
            }

            switch (queryProjection)
            {
                case RestQueryableTreeParser.QueryProjection.Enumerable:
                    var result = this.client.Get<IList<T>>(uri, requestOptions);
                    if (clientSideSelectPart != null)
                        return result.Select(clientSideSelectPart).ToList();
                    return result;
                case RestQueryableTreeParser.QueryProjection.First:
                    return GetFirst(uri, requestOptions, clientSideSelectPart);
                case RestQueryableTreeParser.QueryProjection.FirstOrDefault:
                case RestQueryableTreeParser.QueryProjection.Single:
                    return GetFirst(uri, requestOptions, clientSideSelectPart);
                case RestQueryableTreeParser.QueryProjection.SingleOrDefault:
                    // TODO: SingleOrDefault is obviously not implemented, has been overlooked [KNS]
                case RestQueryableTreeParser.QueryProjection.Max:
                case RestQueryableTreeParser.QueryProjection.Min:
                case RestQueryableTreeParser.QueryProjection.Sum:
                case RestQueryableTreeParser.QueryProjection.Count:
                    return this.client.Get<T>(uri, requestOptions);
                case RestQueryableTreeParser.QueryProjection.Any:
                    // TODO: Implement count querying without returning any results..
                    return this.client.Get<IList<T>>(uri, requestOptions).Count > 0;
                default:
                    throw new NotImplementedException("Don't recognize projection type " + queryProjection);
            }
        }


        private object ExecuteWithClientSelectPart<TServer, TClient>(string uri,
                                                                     RestQueryableTreeParser.QueryProjection
                                                                         queryProjection,
                                                                     Expression<Func<TServer, TClient>>
                                                                         clientSideExpression,
                                                                     RequestOptions requestOptions)
        {
            return Execute(uri, queryProjection, clientSideExpression.Compile(), requestOptions);
        }


        private object GetFirst<T, TConverted>(string uri, RequestOptions requestOptions, Func<T, TConverted> clientSideSelectPart)
        {
            try
            {
                var result = this.client.Get<T>(uri, requestOptions);
                if (clientSideSelectPart != null)
                    return clientSideSelectPart(result);
                return result;
            }
            catch (ResourceNotFoundException ex)
            {
                throw new InvalidOperationException("Sequence contains no matching element", ex);
            }
        }
    }
}