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

using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.Common.Linq.Queries.Interception;

namespace Pomona.UnitTests.Linq.Queries.Interception
{
    [TestFixture]
    public class InterceptedQueryProviderTests
    {
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
    }
}