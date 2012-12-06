using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Internals;

namespace Pomona.Common.Linq
{
    public class RestQueryProvider : IQueryProvider
    {
        private static MethodInfo executeGenericMethod;
        private readonly IPomonaClient client;


        static RestQueryProvider()
        {
            executeGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<RestQueryProvider>(x => x.Execute<object>(null));
        }


        public RestQueryProvider(IPomonaClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            this.client = client;
        }


        public virtual object Execute(Expression expression)
        {
            var elementType = GetElementType(expression.Type);
            var queryTreeParser = new RestQueryableTreeParser();
            queryTreeParser.Visit(expression);
            return executeGenericMethod.MakeGenericMethod(queryTreeParser.SelectReturnType).Invoke(
                this, new object[] { queryTreeParser });
        }


        public string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }


        private static Type GetElementType(Type type)
        {
            if (type.MetadataToken != typeof(IQueryable<>).MetadataToken)
                return type;

            return type.GetGenericArguments()[0];
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

        private string BuildUri(RestQueryableTreeParser parser)
        {
            var builder = new UriQueryBuilder();

            // TODO: Support expand

            if (parser.WherePredicate != null)
            {
                builder.AppendExpressionParameter("$filter", parser.WherePredicate);
            }
            if (parser.OrderKeySelector != null)
            {
                var sortOrder = parser.SortOrder;
                builder.AppendExpressionParameter("$orderby", parser.OrderKeySelector, x => sortOrder == SortOrder.Descending ? x + " desc" : x);
            }
            if (parser.GroupByKeySelector != null)
            {
                builder.AppendExpressionParameter("$groupby", parser.GroupByKeySelector);
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

            return client.GetUriOfType(parser.ElementType) + "?" + builder;
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
    }
}