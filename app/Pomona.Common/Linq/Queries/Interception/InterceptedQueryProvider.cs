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

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries.Interception
{
    public class InterceptedQueryProvider : QueryProviderBase
    {
        private static readonly Func<Type, IQueryProvider, Expression, object> queryableExecuteGenericMethod;
        private readonly IEnumerable<ExpressionVisitor> visitors;

        public event EventHandler<QueryExecutingEventArgs> Executing;


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
                throw new ArgumentNullException("visitors");
            this.visitors = visitors.ToList();
        }


        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new InterceptedQueryable<TElement>(this, expression);
        }


        private static Func<Type, InterceptedQueryProvider, Func<Type, IQueryable>, IQueryable> createLazySource;

        public IQueryable CreateLazySource(Type elementType, Func<Type, IQueryable> factory)
        {
            if (elementType == null)
                throw new ArgumentNullException("elementType");
            return createLazySource(elementType, this, factory);
        }


        public IQueryable<TElement> CreateLazySource<TElement>(Func<Type, IQueryable> factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            return new LazyInterceptedQueryableSource<TElement>(this, factory);
        }


        public override object Execute(Expression expression, Type returnType)
        {
            foreach (ExpressionVisitor visitor in this.visitors)
            {
                expression = visitor.Visit(expression);
            }

            SourceReplaceVisitor sourceReplaceVisitor = new SourceReplaceVisitor();
            expression = sourceReplaceVisitor.Visit(expression);

            var eh = Executing;
            if (eh != null)
            {
                eh(this, new QueryExecutingEventArgs(expression));
            }
            return queryableExecuteGenericMethod(returnType, sourceReplaceVisitor.WrappedProvider, expression);
        }

        #region Nested type: SourceReplaceVisitor

        private class SourceReplaceVisitor : ExpressionVisitor
        {
            private IQueryProvider wrappedProvider;

            public IQueryProvider WrappedProvider
            {
                get { return this.wrappedProvider; }
            }


            protected override Expression VisitConstant(ConstantExpression node)
            {
                var source = node.Value as IInterceptedQueryableSource;
                if (source != null)
                {
                    this.wrappedProvider = source.WrappedSource.Provider;
                    return Expression.Constant(source.WrappedSource, node.Type);
                }
                return base.VisitConstant(node);
            }
        }

        #endregion
    }
}