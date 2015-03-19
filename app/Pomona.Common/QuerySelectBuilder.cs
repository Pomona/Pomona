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

using Pomona.Common.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public class QuerySelectBuilder : QueryPredicateBuilder
    {
        public QuerySelectBuilder(ParameterExpression thisParameter = null)
            : base(thisParameter)
        {
        }


        public new static Expression Create(LambdaExpression lambda)
        {
            return new QuerySelectBuilder().Build(lambda);
        }


        public new static Expression Create<T>(Expression<Func<T, bool>> lambda)
        {
            return Create((LambdaExpression)lambda);
        }


        public new static Expression Create<T, TResult>(Expression<Func<T, TResult>> lambda)
        {
            return Create((LambdaExpression)lambda);
        }


        protected override Expression VisitRootLambda<T>(Expression<T> node)
        {
            var visited = VisitRootLambdaInner(node);
            var pomonaExpr = visited as PomonaExtendedExpression;
            if (pomonaExpr != null && !pomonaExpr.SupportedOnServer)
                return new SelectClientServerPartitionerVisitor(this).SplitExpression(node);
            return visited;
        }


        private Expression VisitRootLambdaInner<T>(Expression<T> node)
        {
            if (node.Body.NodeType == ExpressionType.New)
                return VisitRootNew((NewExpression)node.Body);
            if (node.Body.NodeType == ExpressionType.ListInit)
                return VisitRootListInit((ListInitExpression)node.Body);
            return Nodes(node, base.VisitRootLambda(node), " as this");
        }


        private Expression VisitRootListInit(ListInitExpression body)
        {
            return Visit(body);
        }


        private Expression VisitRootNew(NewExpression node)
        {
            if (!(node.Constructor.DeclaringType.IsAnonymous() || node.Constructor.DeclaringType.IsTuple()))
                return base.Visit(node);

            var readOnlyCollection = node.Members != null
                ? node.Members.Select(x => x.Name)
                : node.Arguments.Select((x, i) => string.Format("Item{0}", i + 1));

            var selectList = node.Arguments.Zip(
                readOnlyCollection,
                (e, p) => new KeyValuePair<string, PomonaExtendedExpression>(p, (PomonaExtendedExpression)Visit(e)))
                .ToList();
            return new QuerySelectExpression(selectList, node.Type);
        }
    }
}