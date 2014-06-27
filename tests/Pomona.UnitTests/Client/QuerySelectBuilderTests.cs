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

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;

namespace Pomona.UnitTests.Client
{
    [TestFixture]
    public class QuerySelectBuilderTests : QueryPredicateBuilderTestsBase
    {
        public class SelectedClientSideClass
        {
            private readonly string bar;


            public SelectedClientSideClass(string bar)
            {
                this.bar = bar;
            }


            public string Bar
            {
                get { return this.bar; }
            }

            public string Foo { get; set; }
        }


        private static Expression<Func<TestResource, T>> Lambda<T>(Expression<Func<TestResource, T>> expr)
        {
            return expr;
        }


        private static PomonaExtendedExpression Build<T>(Expression<Func<TestResource, T>> expr)
        {
            var builder = new QuerySelectBuilder();
            var visited = builder.Build(expr);
            return visited;
        }


        private SelectedClientSideClass ClientMethod1(int theId)
        {
            throw new NotImplementedException("Only placed here as example.");
        }


        private SelectedClientSideClass ClientMethod2(int theId, string foo, string bar)
        {
            throw new NotImplementedException("Only placed here as example.");
        }


        protected class SplitExpressionAsserter<TRet>
        {
            private readonly Expression<Func<TestResource, TRet>> expr;


            public SplitExpressionAsserter(Expression<Func<TestResource, TRet>> expr)
            {
                if (expr == null)
                    throw new ArgumentNullException("expr");
                this.expr = expr;
            }


            public void Expect<TTuple>(string expectedServerPart, Expression<Func<TTuple, TRet>> expectedClientPart)
            {
                var node = AssertAndCastToSplitSelectExpression(Build(this.expr));
                Assert.AreEqual(expectedServerPart, node.ServerExpression.ToString());
                AssertExpressionEquals((Expression<Func<TTuple, TRet>>)node.ClientSideExpression,
                    expectedClientPart);
            }
        }


        protected SplitExpressionAsserter<TRet> BuildAndAssertSplitExpression<TRet>(
            Expression<Func<TestResource, TRet>> expr)
        {
            return new SplitExpressionAsserter<TRet>(expr);
        }


        private static ClientServerSplitSelectExpression AssertAndCastToSplitSelectExpression(
            PomonaExtendedExpression pomonaExtendedExpression)
        {
            var nodeUncast = pomonaExtendedExpression;
            Assert.IsInstanceOf<ClientServerSplitSelectExpression>(nodeUncast);
            var node = (ClientServerSplitSelectExpression)nodeUncast;
            return node;
        }


        private IEnumerable<T> ClientEnumerableFilter<T>(IEnumerable<T> source)
        {
            throw new NotImplementedException("Only here as example for test.");
        }


        [Test]
        public void Build_FromExpression_ReferencingOneServerProperty_PassedToMethodOnClient_SplitsServerAndClientPart()
        {
            BuildAndAssertSplitExpression(x => ClientMethod1(x.Id)).Expect<Tuple<int>>("id as Item1",
                _this => ClientMethod1(_this.Item1));
        }


        [Test]
        public void Build_FromExpression_SelectingValue_WhereAllNodesAreSupportedOnServer_DoesNotSplitExpression()
        {
            var node = Build(x => x.Bonga);
            Assert.That(node, Is.Not.InstanceOf<ClientServerSplitSelectExpression>());
            Assert.That(node, Is.InstanceOf<QuerySegmentExpression>());
        }


        [Test]
        public void
            Build_FromSplittableExpression_ReferencingSameServerPropertyMultipleTimes_PassedToMethodOnClient_DoesNotRepeatPropertyInSelect
            ()
        {
            BuildAndAssertSplitExpression(x => ClientMethod2(x.Id, x.Bonga, x.Bonga)).Expect<Tuple<int, string>>(
                "id as Item1,bonga as Item2",
                _this => ClientMethod2(_this.Item1, _this.Item2, _this.Item2));
        }


        [Test]
        public void Build_FromSplittableExpression_WithClientSideCountSubQuery_PassedToMethodOnClient_RunsQueryOnClient()
        {
            BuildAndAssertSplitExpression(
                x => ClientMethod1(ClientEnumerableFilter(x.SomeList).Count(y => y.SomeDouble > 1.0)))
                .Expect<Tuple<IList<FooBar>>>("someList as Item1",
                    _this => ClientMethod1(ClientEnumerableFilter(_this.Item1).Count(y => y.SomeDouble > 1.0)));
        }


        [Test]
        public void Build_FromSplittableExpression_WithCountSubQuery_PassedToMethodOnClient_RunsQueryOnServer()
        {
            BuildAndAssertSplitExpression(x => ClientMethod1(x.SomeList.Count()))
                .Expect<Tuple<int>>("count(someList) as Item1",
                    _this => ClientMethod1(_this.Item1));
        }


        [Test]
        public void Build_FromSplittableExpression_WithPredicatedCountSubQuery_PassedToMethodOnClient_RunsQueryOnServer()
        {
            BuildAndAssertSplitExpression(x => ClientMethod1(x.SomeList.Count(y => y.SomeDouble > 1.0)))
                .Expect<Tuple<int>>("count(someList,y:y.someDouble gt 1.0) as Item1",
                    _this => ClientMethod1(_this.Item1));
        }


        [Test]
        public void Build_FromSplittableExpression_WithStringConcat_PassedToMethodOnClient_ConcatsOnClient()
        {
            BuildAndAssertSplitExpression(x => ClientMethod2(x.Id, x.Bonga, x.Bonga + x.Jalla))
                .Expect<Tuple<int, string, string>>("id as Item1,bonga as Item2,jalla as Item3",
                    _this => ClientMethod2(_this.Item1, _this.Item2, string.Concat(_this.Item2, _this.Item3)));
        }
    }
}