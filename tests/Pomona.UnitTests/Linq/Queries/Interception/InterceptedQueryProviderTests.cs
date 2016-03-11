#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.Common.Linq.Queries.Interception;

namespace Pomona.UnitTests.Linq.Queries.Interception
{
    [TestFixture]
    public class InterceptedQueryProviderTests
    {
        [Test]
        public void InterceptWith_Replaces_Source_With_Wrapped_Source_On_Execution()
        {
            Assert.That(new[] { 1337 }.AsQueryable().InterceptWith().FirstOrDefault(), Is.EqualTo(1337));
        }


        [Test]
        public void InterceptWith_Runs_Visitors_In_Order()
        {
            Assert.That(
                new[] { 0 }.AsQueryable()
                           .InterceptWith(new IncrementIntConstantVisitor(),
                                          new DoubleIntConstantVisitor()).Select(x => 8)
                           .FirstOrDefault(),
                Is.EqualTo(18));
            Assert.That(
                new[] { 0 }.AsQueryable().
                            InterceptWith(new DoubleIntConstantVisitor(),
                                          new IncrementIntConstantVisitor())
                           .Select(x => 8)
                           .FirstOrDefault(),
                Is.EqualTo(17));
        }


        private class DoubleIntConstantVisitor : ExpressionVisitor
        {
            protected override Expression VisitConstant(ConstantExpression node)
            {
                var value = node.Value as int?;
                if (value.HasValue)
                    return Expression.Constant(value * 2, node.Type);
                return base.VisitConstant(node);
            }
        }

        private class IncrementIntConstantVisitor : ExpressionVisitor
        {
            protected override Expression VisitConstant(ConstantExpression node)
            {
                var value = node.Value as int?;
                if (value.HasValue)
                    return Expression.Constant(value + 1, node.Type);
                return base.VisitConstant(node);
            }
        }
    }
}