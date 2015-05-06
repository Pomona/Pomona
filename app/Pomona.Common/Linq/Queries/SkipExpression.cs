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
    public class SkipExpression : QueryChainedExpression
    {
        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Skip(0));

        private static SkipFactory factory;


        private SkipExpression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {
        }


        public int Count
        {
            get { return (int)((ConstantExpression)Arguments[1]).Value; }
        }

        public static QueryExpressionFactory Factory
        {
            get { return factory ?? (factory = new SkipFactory()); }
        }


        public static SkipExpression Create(QueryExpression source, int count)
        {
            return factory.Create(source, count);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var origSource = Source;
            var visitedSource = visitor.VisitAndConvert(origSource, "VisitChildren");
            if (visitedSource != origSource)
                return Create(visitedSource, Count);
            return this;
        }

        #region Nested type: SkipFactory

        private class SkipFactory : QueryChainedExpressionFactory<SkipExpression>
        {
            public SkipExpression Create(QueryExpression source, int count)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                return new SkipExpression(Call(Method.MakeGenericMethod(source.ElementType),
                                               source.Node,
                                               Constant(count)),
                                          source);
            }
        }

        #endregion
    }
}