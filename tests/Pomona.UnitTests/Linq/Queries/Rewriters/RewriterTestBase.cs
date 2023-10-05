#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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


        protected abstract IExpressionRewriter CreateRewriter();


        protected static IQueryable<T> Q<T>()
        {
            return Enumerable.Empty<T>().AsQueryable();
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

