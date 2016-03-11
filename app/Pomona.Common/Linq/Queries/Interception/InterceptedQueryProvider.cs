#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries.Interception
{
    using LazySource = Func<Type, InterceptedQueryProvider, Func<Type, IQueryable>, IQueryable>;
    using QueryableExecuteGenericMethod = Func<Type, IQueryProvider, Expression, object>;

    public class InterceptedQueryProvider : QueryProviderBase
    {
        private static readonly LazySource createLazySource;
        private static readonly QueryableExecuteGenericMethod queryableExecuteGenericMethod;
        private readonly IEnumerable<ExpressionVisitor> visitors;


        static InterceptedQueryProvider()
        {
            createLazySource = GenericInvoker
                .Instance<InterceptedQueryProvider>()
                .CreateFunc1<Func<Type, IQueryable>, IQueryable>(x => x.CreateLazySource<object>(null));

            queryableExecuteGenericMethod = GenericInvoker
                .Instance<IQueryProvider>()
                .CreateFunc1<Expression, object>(x => x.Execute<object>(null));
        }


        public InterceptedQueryProvider(IEnumerable<ExpressionVisitor> visitors)
        {
            if (visitors == null)
                throw new ArgumentNullException(nameof(visitors));
            this.visitors = visitors.ToList();
        }


        public IQueryable CreateLazySource(Type elementType, Func<Type, IQueryable> factory)
        {
            if (elementType == null)
                throw new ArgumentNullException(nameof(elementType));
            return createLazySource(elementType, this, factory);
        }


        public IQueryable<TElement> CreateLazySource<TElement>(Func<Type, IQueryable> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new LazyInterceptedQueryableSource<TElement>(this, factory);
        }


        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new InterceptedQueryable<TElement>(this, expression);
        }


        public override object Execute(Expression expression, Type returnType)
        {
            foreach (ExpressionVisitor visitor in this.visitors)
                expression = visitor.Visit(expression);

            SourceReplaceVisitor sourceReplaceVisitor = new SourceReplaceVisitor();
            expression = sourceReplaceVisitor.Visit(expression);

            var eh = Executing;
            if (eh != null)
                eh(this, new QueryExecutingEventArgs(expression));
            return queryableExecuteGenericMethod(returnType, sourceReplaceVisitor.WrappedProvider, expression);
        }


        public event EventHandler<QueryExecutingEventArgs> Executing;

        #region Nested type: SourceReplaceVisitor

        private class SourceReplaceVisitor : ExpressionVisitor
        {
            public IQueryProvider WrappedProvider { get; private set; }


            protected override Expression VisitConstant(ConstantExpression node)
            {
                var source = node.Value as IInterceptedQueryableSource;
                if (source != null)
                {
                    WrappedProvider = source.WrappedSource.Provider;
                    return Expression.Constant(source.WrappedSource, node.Type);
                }
                return base.VisitConstant(node);
            }
        }

        #endregion
    }
}