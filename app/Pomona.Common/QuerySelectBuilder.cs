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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

        new public static string Create(LambdaExpression lambda)
        {
            return new QuerySelectBuilder().Visit(lambda).ToString();
        }


        new public static string Create<T>(Expression<Func<T, bool>> lambda)
        {
            return Create((LambdaExpression)lambda);
        }


        new public static string Create<T, TResult>(Expression<Func<T, TResult>> lambda)
        {
            return Create((LambdaExpression)lambda);
        }


        protected override Expression VisitRootLambda<T>(Expression<T> node)
        {
            if (node.Body.NodeType == ExpressionType.New)
            {
                return VisitRootNew((NewExpression)node.Body);
            }
            if (node.Body.NodeType == ExpressionType.ListInit)
            {
                return VisitRootListInit((ListInitExpression)node.Body);
            }
            return Nodes(base.VisitRootLambda(node), " as this");
        }


        private Expression VisitRootListInit(ListInitExpression body)
        {
            if (body.Type != typeof(Dictionary<string, object>))
                return Visit(body);

            var pairs = body.Initializers.Select(
                x =>
                    new
                    {
                        Key =
                            x.Arguments.ElementAtOrDefault(0).Maybe().Where(y => y.Type == typeof(string))
                                .OfType<ConstantExpression>().Select(y => (string)y.Value).OrDefault(),
                        Value = x.Arguments.ElementAtOrDefault(1)
                    }).ToList();

            if (pairs.Any(x => x.Key == null || x.Value == null))
                return Visit(body);

            var children = new List<object>();
            foreach (var kvp in pairs)
            {
                if (children.Count > 0)
                    children.Add(",");

                children.Add(Visit(kvp.Value));
                children.Add(" as ");
                children.Add(kvp.Key);
            }

            return Nodes(children);
        }


        private Expression VisitRootNew(NewExpression node)
        {
            var children = new List<object>();

            var readOnlyCollection = node.Members != null
                ? node.Members.Select(x => x.Name)
                : node.Arguments.Select((x, i) => string.Format("Item{0}", i + 1));
            foreach (
                var arg in
                    node.Arguments.Zip(
                        readOnlyCollection, (e, p) => new { Name = p, Expr = e }))
            {
                if (children.Count > 0)
                    children.Add(",");

                children.Add(Visit(arg.Expr));
                children.Add(" as ");
                children.Add(arg.Name);
            }

            return Nodes(children);
        }
    }
}