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

using System;
using System.Linq.Expressions;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    public class QueryFilterExpressionParser
    {
        private readonly IQueryPropertyResolver queryPropertyResolver;


        public QueryFilterExpressionParser(IQueryPropertyResolver queryPropertyResolver)
        {
            if (queryPropertyResolver == null)
                throw new ArgumentNullException("queryPropertyResolver");
            this.queryPropertyResolver = queryPropertyResolver;
        }


        public Expression<Func<T, bool>> Parse<T>(string odataExpression)
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

            var nodeTreeToExpressionConverter = new NodeTreeToExpressionConverter<T>(queryPropertyResolver);

            return nodeTreeToExpressionConverter.ToLambdaExpression(tempTree);
        }
    }
}