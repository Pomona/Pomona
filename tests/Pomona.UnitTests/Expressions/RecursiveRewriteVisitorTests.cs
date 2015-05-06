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
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.Common.Expressions;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Expressions
{
    [TestFixture]
    public class RecursiveRewriteVisitorTests
    {
        private readonly IExpressionRewriter noopRewriter = ExpressionRewriter.Create(x => x);

        private readonly IExpressionRewriter stringConstantToUpperRewriter =
            ExpressionRewriter.Create<ConstantExpression>(x =>
            {
                var valueAsString = x.Value as string;
                if (valueAsString == null)
                    return x;
                var upperString = valueAsString.ToUpperInvariant();
                return valueAsString != upperString ? Expression.Constant(upperString) : x;
            });

        private readonly IExpressionRewriter trimStringRewriter =
            ExpressionRewriter.Create<ConstantExpression>(x =>
            {
                var valueAsString = x.Value as string;
                if (valueAsString == null)
                    return x;
                var trimmedString = valueAsString.Trim();
                return valueAsString != trimmedString ? Expression.Constant(trimmedString) : x;
            });


        [Test]
        public void Visit_Expression_MatchingMultipleRules_ReturnsVisitedExpression()
        {
            AssertRewrite(() => "     lala   ",
                          () => "LALA",
                          this.stringConstantToUpperRewriter,
                          this.trimStringRewriter);
        }


        [Test]
        public void Visit_Expression_MatchingNoRules_ReturnsUnmodifiedExpression()
        {
            var visitor = new RecursiveRewriteVisitor(this.noopRewriter);
            var origNode = Expression.Constant("Whatever");
            var visited = visitor.Visit(origNode);
            Assert.That(visited, Is.EqualTo(origNode));
        }


        [Test]
        public void Visit_Expression_MatchingSingleRule_ReturnsVisitedExpression()
        {
            AssertRewrite(() => "lala", () => "LALA", this.stringConstantToUpperRewriter);
        }


        private void AssertRewrite<TInput, TExpected>(Expression<Func<TInput>> input,
                                                      Expression<Func<TExpected>> expected,
                                                      params IExpressionRewriter[] rewriters)
        {
            var visitor = new RecursiveRewriteVisitor(rewriters);
            var visitedBody = visitor.Visit(input.Body);
            visitedBody.AssertEquals(expected.Body);
        }
    }
}