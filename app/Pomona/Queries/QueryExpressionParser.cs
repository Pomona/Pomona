#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Pomona.CodeGen;
using Pomona.Common.Internals;

namespace Pomona.Queries
{
    public class QueryExpressionParser
    {
        private readonly IQueryTypeResolver queryPropertyResolver;


        public QueryExpressionParser(IQueryTypeResolver queryPropertyResolver)
        {
            if (queryPropertyResolver == null)
                throw new ArgumentNullException("queryPropertyResolver");
            this.queryPropertyResolver = queryPropertyResolver;
        }


        public Expression<Func<T, bool>> Parse<T>(string odataExpression)
        {
            return (Expression<Func<T, bool>>) Parse(typeof (T), odataExpression);
        }

        public LambdaExpression ParseSelectList(Type thisType, string selectListExpression)
        {
            // By enclosing the selectListExpression in a method call, we can use the same parser
            // as for filter queries, then we convert each part of the select statement to a
            // lambda expression.
            var symbolTree = ParseSymbolTree(string.Format("select({0})", selectListExpression));
            ParameterExpression thisParam = Expression.Parameter(thisType, "_this");
            var selectParts = symbolTree.Children.Select(x => ParseSelectPart(thisType, x, thisParam)).ToList();

            if (selectParts.Count == 1 && selectParts[0].Key.ToLower() == "this")
            {
                // This is a way to select a list of one property, without encapsulating it inside anon objects..
                return Expression.Lambda(selectParts[0].Value, thisParam);
            }

            Type anonTypeInstance;
            var expr = AnonymousTypeBuilder.CreateNewExpression(selectParts, out anonTypeInstance);
            return Expression.Lambda(expr, thisParam);
        }

        private KeyValuePair<string, Expression> ParseSelectPart(Type thisType, NodeBase node, ParameterExpression thisParam)
        {
            string propertyName = null;
            if (node.NodeType == NodeType.As)
            {
                var asNode = (BinaryOperator) node;
                if (asNode.Right.NodeType != NodeType.Symbol)
                {
                    throw new PomonaExpressionSyntaxException(
                        "Need to specify a property name on left side of as in select expression.");
                }
                propertyName = ((SymbolNode) asNode.Right).Name;
                node = asNode.Left;
            }
            else
            {
                if (!TryGetImplicitPropertyName(node, out propertyName))
                    throw new PomonaExpressionSyntaxException(
                        string.Format("Unable to infer property name of select expression ({0})", node));
            }

            var exprParser = new NodeTreeToExpressionConverter(queryPropertyResolver);
            var lambdaExpression = exprParser.ToLambdaExpression(thisParam, thisParam.WrapAsEnumerable(),
                                                                 Enumerable.Empty<ParameterExpression>(), node);

            return new KeyValuePair<string, Expression>(propertyName, lambdaExpression.Body);
        }

        private bool TryGetImplicitPropertyName(NodeBase node, out string propertyName)
        {
            propertyName = null;
            while (node.NodeType == NodeType.Dot)
                node = ((BinaryOperator) node).Right;

            if (node.NodeType != NodeType.Symbol)
                return false;

            propertyName = ((SymbolNode) node).Name;
            return true;
        }

        public LambdaExpression Parse(Type thisType, string odataExpression)
        {
            var tempTree = ParseSymbolTree(odataExpression);

            var nodeTreeToExpressionConverter = new NodeTreeToExpressionConverter(queryPropertyResolver);

            var lambdaExpression = nodeTreeToExpressionConverter.ToLambdaExpression(thisType, tempTree);
            return lambdaExpression;
        }

        private static NodeBase ParseSymbolTree(string odataExpression)
        {
            if (odataExpression == null)
                throw new ArgumentNullException("odataExpression");
            var input = new ANTLRStringStream(odataExpression);

            var lexer = new PomonaQueryLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new PomonaQueryParser(tokens);
            var parseReturn = parser.parse();
            var tree = (CommonTree) parseReturn.Tree;

            var tempTree = PomonaQueryTreeParser.ParseTree(tree, 0);
            return tempTree;
        }
    }
}