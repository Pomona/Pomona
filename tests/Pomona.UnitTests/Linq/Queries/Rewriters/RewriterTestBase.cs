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
using System.Linq;
using System.Linq.Expressions;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common.Expressions;
using Pomona.Common.Internals;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Linq.Queries.Rewriters
{
    public abstract class RewriterTestBase<TRewriter> : RewriterTestBase
        where TRewriter : IExpressionRewriter, new()
    {
        protected readonly IQueryable<Animal> Animals = Q<Animal>();

        protected override IExpressionRewriter CreateRewriter()
        {
            return new TRewriter();
        }
    }

    public abstract class RewriterTestBase
    {
        protected virtual IRewriteContext Context { get; set; }
        protected virtual IExpressionRewriter Rewriter { get; set; }


        [SetUp]
        public virtual void SetUp()
        {
            Rewriter = CreateRewriter();
            Context = Substitute.For<IRewriteContext>();
        }


        protected abstract IExpressionRewriter CreateRewriter();


        protected static IQueryable<T> Q<T>()
        {
            return Enumerable.Empty<T>().AsQueryable();
        }


        protected void AssertDoesNotRewrite<TInput>(Expression<Func<TInput>> input)
        {
            var closureEvaluator = new EvaluateClosureMemberVisitor();
            var body = closureEvaluator.Visit(input.Body);
            var visitedBody = Rewriter.Visit(Context, body);
            Assert.That(body, Is.EqualTo(visitedBody), "Visitor modified expression.");
        }


        protected void AssertRewrite<TInput, TExpected>(Expression<Func<TInput>> input,
                                                        Expression<Func<TExpected>> expected)
        {
            var closureEvaluator = new EvaluateClosureMemberVisitor();
            var visitedBody = Rewriter.Visit(Context, closureEvaluator.Visit(input.Body));
            visitedBody.AssertEquals(closureEvaluator.Visit(expected.Body));
        }

        protected void AssertRewriteRecursive<TInput, TExpected>(Expression<Func<TInput>> input,
                                                        Expression<Func<TExpected>> expected)
        {
            var closureEvaluator = new EvaluateClosureMemberVisitor();
            var recursiveRewriter = new RecursiveRewriteVisitor(Rewriter);
            var visitedBody = recursiveRewriter.Visit(closureEvaluator.Visit(input.Body));
            visitedBody.AssertEquals(closureEvaluator.Visit(expected.Body));
        }

        #region Nested type: Animal

        public class Animal
        {
            public int Id { get; set; }
        }

        #endregion

        public class Dog : Animal
        {
        }
    }
}