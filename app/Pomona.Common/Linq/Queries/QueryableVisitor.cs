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

using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public class QueryableVisitor
        : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            QueryExpression qn;
            if (QueryExpression.TryWrap(node, out qn))
                return Visit(qn).ReduceExtensions();
            var visited = base.Visit(node);
            if (visited != node)
                return Visit(visited);
            return visited;
        }


        protected override Expression VisitExtension(Expression node)
        {
            var ofTypeExpression = node as OfTypeExpression;
            if (ofTypeExpression != null)
                return VisitOfType(ofTypeExpression);
            var whereExpression = node as WhereExpression;
            if (whereExpression != null)
                return VisitWhere(whereExpression);
            var selectManyExpression = node as SelectManyExpression;
            if (selectManyExpression != null)
                return VisitSelectMany(selectManyExpression);
            var selectExpression = node as SelectExpression;
            if (selectExpression != null)
                return VisitSelect(selectExpression);
            var querySourceExpression = node as QuerySourceExpression;
            if (querySourceExpression != null)
                return VisitQuerySource(querySourceExpression);
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitOfType(OfTypeExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitQuerySource(QuerySourceExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitSelect(SelectExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitSelectMany(SelectManyExpression node)
        {
            return base.VisitExtension(node);
        }


        protected virtual Expression VisitWhere(WhereExpression node)
        {
            return base.VisitExtension(node);
        }
    }
}