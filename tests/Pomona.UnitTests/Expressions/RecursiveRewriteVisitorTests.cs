#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
