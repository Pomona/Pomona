#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries
{
    public class SelectExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Select(y => false));

        private static SelectFactory factory;


        private SelectExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public static QueryExpressionFactory Factory
        {
            get { return factory ?? (factory = new SelectFactory()); }
        }

        public LambdaExpression Selector
        {
            get { return (LambdaExpression)((UnaryExpression)Arguments[1]).Operand; }
        }


        public static SelectExpression Create(QueryExpression source, LambdaExpression selector)
        {
            return factory.Create(source, selector);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var origSelector = Selector;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitSelect");
            var visitedSelector = visitor.VisitAndConvert(origSelector, "VisitSelect");
            if (visitedSource != origSource || visitedSelector != origSelector)
                return factory.Create(visitedSource, visitedSelector);
            return this;
        }

        #region Nested type: WhereFactory

        internal class SelectFactory : QueryChainedExpressionFactory<SelectExpression>
        {
            public SelectExpression Create(QueryExpression source, LambdaExpression selector)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (selector == null)
                    throw new ArgumentNullException("selector");
                return new SelectExpression(Call(Method.MakeGenericMethod(source.ElementType, selector.ReturnType),
                                                 source.Node,
                                                 ConvertAndQuote(selector, source.ElementType)),
                                            source);
            }
        }

        #endregion
    }
}