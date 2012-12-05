using System;
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
            var queryTreeParser = new RestQueryableTreeParser();
            queryTreeParser.Visit(expression);
            return executeGenericMethod.MakeGenericMethod(queryTreeParser.ElementType).Invoke(
                this, new object[] { queryTreeParser });
        }


        public string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }


        private static Type GetElementType(Type type)
        {
            if (type.MetadataToken != typeof(IQueryable<>).MetadataToken)
                throw new NotSupportedException("Don't know how to get element type from " + type.FullName);

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


        private object Execute<T>(RestQueryableTreeParser parser)
        {
            var wherePredicate = (Expression<Func<T, bool>>)parser.WherePredicate;

            var orderKeySelector = parser.OrderKeySelector;
            if (orderKeySelector != null)
            {
                if (orderKeySelector.Type != typeof(Expression<Func<T, object>>))
                {
                    // Must convert to object to work with Query method of function
                    orderKeySelector =
                        Expression.Lambda<Func<T, object>>(
                            Expression.Convert(orderKeySelector.Body, typeof(object)), orderKeySelector.Parameters);
                }
            }

            var results = this.client.Query<T>(
                wherePredicate,
                (Expression<Func<T, object>>)orderKeySelector,
                parser.SortOrder,
                parser.TakeCount,
                parser.SkipCount);

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