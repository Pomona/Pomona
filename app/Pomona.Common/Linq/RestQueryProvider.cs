#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Internals;

namespace Pomona.Common.Linq
{
    public class RestQueryProvider : IQueryProvider
    {
        private static readonly MethodInfo executeGenericMethod;
        private static readonly MethodInfo mapToCustomUserTypeResultMethod;
        private readonly IPomonaClient client;

        private readonly Type sourceType;


        static RestQueryProvider()
        {
            executeGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<RestQueryProvider>(x => x.Execute<object>(null));
            mapToCustomUserTypeResultMethod =
                ReflectionHelper.GetGenericMethodDefinition<RestQueryProvider>(
                    x => x.MapToCustomUserTypeResult<object>(null, null, null, null));
        }


        internal RestQueryProvider(IPomonaClient client, Type sourceType)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");
            this.client = client;
            this.sourceType = sourceType;
        }


        internal IPomonaClient Client
        {
            get { return this.client; }
        }


        public virtual object Execute(Expression expression)
        {
            object result;
            if (TryQueryCustomUserTypeIfRequired(expression, this.sourceType, out result))
                return result;

            var queryTreeParser = new RestQueryableTreeParser();
            queryTreeParser.Visit(expression);
            return executeGenericMethod.MakeGenericMethod(queryTreeParser.SelectReturnType).Invoke(
                this, new object[] { queryTreeParser });
        }


        public string GetQueryText(Expression expression)
        {
            return expression.ToString();
        }


        private static Type GetElementType(Type type)
        {
            if (type.MetadataToken != typeof(IQueryable<>).MetadataToken)
                return type;

            return type.GetGenericArguments()[0];
        }


        private string BuildUri(RestQueryableTreeParser parser)
        {
            var builder = new UriQueryBuilder();

            // TODO: Support expand

            var resourceInfo = client.GetResourceInfoForType(sourceType);

            if (!resourceInfo.IsUriBaseType)
                builder.AppendParameter("$oftype", resourceInfo.JsonTypeName);

            if (parser.WherePredicate != null)
                builder.AppendExpressionParameter("$filter", parser.WherePredicate);
            if (parser.OrderKeySelector != null)
            {
                var sortOrder = parser.SortOrder;
                builder.AppendExpressionParameter(
                    "$orderby", parser.OrderKeySelector, x => sortOrder == SortOrder.Descending ? x + " desc" : x);
            }
            if (parser.GroupByKeySelector != null)
                builder.AppendExpressionParameter("$groupby", parser.GroupByKeySelector);
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

            return this.client.GetUriOfType(parser.ElementType) + "?" + builder;
        }


        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return new RestQuery<S>(this, expression);
        }


        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var elementType = GetElementType(expression.Type);
            try
            {
                return
                    (IQueryable)
                    Activator.CreateInstance(
                        typeof(RestQuery<>).MakeGenericType(elementType),
                        new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }


        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)Execute(expression);
        }


        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }


        private object Execute<T>(RestQueryableTreeParser parser)
        {
            var results = this.client.Get<IList<T>>(BuildUri(parser));

            switch (parser.Projection)
            {
                case RestQueryableTreeParser.QueryProjection.Enumerable:
                    return results;
                case RestQueryableTreeParser.QueryProjection.FirstOrDefault:
                    return results.FirstOrDefault();
                case RestQueryableTreeParser.QueryProjection.First:
                    return results.First();
                case RestQueryableTreeParser.QueryProjection.Any:
                    // TODO: Implement count querying without returning any results..
                    return results.Count > 0;
                default:
                    throw new NotImplementedException("Don't recognize projection type " + parser.Projection);
            }
        }


        private PropertyInfo GetAttributesDictionaryPropertyFromResource(Type serverKnownType)
        {
            var attrProp =
                serverKnownType.GetAllInheritedPropertiesFromInterface().FirstOrDefault(
                    x => x.GetCustomAttributes(typeof(ResourceAttributesPropertyAttribute), true).Any());

            if (attrProp == null)
            {
                throw new InvalidOperationException(
                    "Unable to find property with ResourceAttributesPropertyAttribute attached to it on type "
                    + serverKnownType.FullName);
            }

            return attrProp;
        }


        private object MapToCustomUserTypeResult<TCustomClientType>(
            object result, Type serverKnownType, PropertyInfo dictProp, Expression transformedExpression)
        {
            Type elementType;
            if (transformedExpression.Type.TryGetCollectionElementType(out elementType)
                && elementType == serverKnownType)
            {
                // Map back to customClientType
                var resultsWrapper =
                    (result as IEnumerable).Cast<object>().Select(
                        x =>
                        {
                            var proxy =
                                (ClientSideResourceProxyBase)
                                ((object)RuntimeProxyFactory<ClientSideResourceProxyBase, TCustomClientType>.Create());
                            proxy.AttributesProperty = dictProp;
                            proxy.ProxyTarget = x;
                            return (TCustomClientType)((object)proxy);
                        }).ToList();

                return resultsWrapper;
            }
            // TODO!
            return result;
        }


        private bool TryQueryCustomUserTypeIfRequired(Expression expression, Type customClientType, out object result)
        {
            var serverKnownType = this.client.GetMostInheritedResourceInterface(customClientType);
            if (customClientType == serverKnownType)
            {
                result = null;
                return false;
            }

            var dictProp = GetAttributesDictionaryPropertyFromResource(serverKnownType);

            var visitor = new TransformAdditionalPropertiesToAttributesVisitor(
                customClientType, serverKnownType, dictProp);
            var transformedExpression = visitor.Visit(expression);

            var nestedQueryProvider = new RestQueryProvider(this.client, serverKnownType);
            result = nestedQueryProvider.Execute(transformedExpression);

            result = mapToCustomUserTypeResultMethod.MakeGenericMethod(customClientType).Invoke(
                this, new[] { result, serverKnownType, dictProp, transformedExpression });

            return true;
        }
    }
}