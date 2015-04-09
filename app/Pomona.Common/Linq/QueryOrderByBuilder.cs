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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    internal class QueryOrderByBuilder : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node.NodeType != ExpressionType.Constant
                || !(typeof(IEnumerable<Tuple<LambdaExpression, SortOrder>>).IsAssignableFrom(node.Type)))
                throw new NotSupportedException("This visitor only supports IEnumerable<Tuple<LambdaExpression, SortOrder>> constant.");

            return base.Visit(node);
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            return Visit((IEnumerable<Tuple<LambdaExpression, SortOrder>>)node.Value);
        }


        private PomonaExtendedExpression Visit(IEnumerable<Tuple<LambdaExpression, SortOrder>> orderKeySelectors)
        {
            return
                new QueryOrderExpression(
                    orderKeySelectors.Select(
                        x =>
                            new Tuple<PomonaExtendedExpression, SortOrder>(
                            (PomonaExtendedExpression)ExpressionExtensions.Visit<QueryPredicateBuilder>(x.Item1), x.Item2)), typeof(object));
        }
    }
}