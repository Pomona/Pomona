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
using System.Reflection;

using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Internals;

namespace Pomona.Common.Linq
{
    public class RestQueryProvider : QueryProviderBase
    {
        private static readonly MethodInfo executeGenericMethod;
        private readonly IPomonaClient client;


        static RestQueryProvider()
        {
            executeGenericMethod =
                ReflectionHelper.GetMethodDefinition<RestQueryProvider>(x => x.Execute<object>(null));
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

            return executeGenericMethod.MakeGenericMethod(queryTreeParser.SelectReturnType).InvokeDirect(
                this,
                queryTreeParser);
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


        private string BuildUri(RestQueryableTreeParser parser)
        {
            var builder = new UriQueryBuilder();

            // TODO: Support expand

            var resourceInfo = this.client.GetResourceInfoForType(parser.ElementType);

            if (!resourceInfo.IsUriBaseType)
                builder.AppendParameter("$oftype", resourceInfo.JsonTypeName);

            SetProjection(parser, builder);

            if (parser.WherePredicate != null)
                builder.AppendExpressionParameter("$filter", parser.WherePredicate);
            if (parser.OrderKeySelector != null)
            {
                var sortOrder = parser.SortOrder;
                builder.AppendExpressionParameter(
                    "$orderby",
                    parser.OrderKeySelector,
                    x => sortOrder == SortOrder.Descending ? x + " desc" : x);
            }
            if (parser.GroupByKeySelector != null)
            {
                var selectBuilder = new QuerySelectBuilder(parser.GroupByKeySelector);
                builder.AppendParameter("$groupby", selectBuilder);
            }
            if (parser.SelectExpression != null)
            {
                var selectBuilder = new QuerySelectBuilder(parser.SelectExpression);
                builder.AppendParameter("$select", selectBuilder);
            }
            if (parser.SkipCount.HasValue)
                builder.AppendParameter("$skip", parser.SkipCount.Value);
            if (parser.TakeCount.HasValue)
                builder.AppendParameter("$top", parser.TakeCount.Value);

            var expandedPaths = parser.ExpandedPaths;
            if (!string.IsNullOrEmpty(expandedPaths))
                builder.AppendParameter("$expand", expandedPaths);

            if (parser.IncludeTotalCount)
                builder.AppendParameter("$totalcount", "true");

            return (parser.RepositoryUri ?? this.client.GetUriOfType(parser.ElementType)) + "?" + builder;
        }


        private object Execute<T>(RestQueryableTreeParser parser)
        {
            var uri = BuildUri(parser);

            if (parser.Projection == RestQueryableTreeParser.QueryProjection.ToJson)
                return this.client.Get<JToken>(uri);

            if (parser.Projection == RestQueryableTreeParser.QueryProjection.ToUri)
                return new Uri(uri);

            if (parser.Projection == RestQueryableTreeParser.QueryProjection.FirstLazy)
            {
                var resourceInfo = this.client.GetResourceInfoForType(typeof(T));
                var proxy = (LazyProxyBase)Activator.CreateInstance(resourceInfo.LazyProxyType);
                proxy.Uri = uri;
                proxy.Client = this.client;
                return proxy;
            }

            switch (parser.Projection)
            {
                case RestQueryableTreeParser.QueryProjection.Enumerable:
                    return this.client.Get<IList<T>>(uri);
                case RestQueryableTreeParser.QueryProjection.First:
                    return GetFirst<T>(uri);
                case RestQueryableTreeParser.QueryProjection.FirstOrDefault:
                case RestQueryableTreeParser.QueryProjection.Max:
                case RestQueryableTreeParser.QueryProjection.Min:
                case RestQueryableTreeParser.QueryProjection.Sum:
                case RestQueryableTreeParser.QueryProjection.Count:
                    return this.client.Get<T>(uri);
                case RestQueryableTreeParser.QueryProjection.Any:
                    // TODO: Implement count querying without returning any results..
                    return this.client.Get<IList<T>>(uri).Count > 0;
                default:
                    throw new NotImplementedException("Don't recognize projection type " + parser.Projection);
            }
        }


        private T GetFirst<T>(string uri)
        {
            try
            {
                return this.client.Get<T>(uri);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Sequence contains no matching element", ex);
            }
        }
    }
}