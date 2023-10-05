#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Linq;

namespace Pomona.UnitTests.Client
{
    [TestFixture]
    public class ClientServerSplittingSelectBuilderTests : QueryPredicateBuilderTestsBase
    {
        [Test]
        public void Build_FromExpression_ReferencingOneServerProperty_PassedToMethodOnClient_SplitsServerAndClientPart()
        {
            BuildAndAssertSplitExpression(x => ClientMethod1(x.Id)).Expect<object[]>("[id] as this",
                                                                                     _this => ClientMethod1((int)_this[0]));
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
            Build_FromSplittableExpression_HavingSubquery_WithWhereGivenServerSupportedLambda_AndCountGivenServerUnsupportedLambda_SubQueryIsSplit
            ()
        {
            BuildAndAssertSplitExpression(
                x => x.SomeList.Where(y => y.SomeInt % 3 == 1).Any(y => ClientMethod1(y.SomeInt).Bar == "JallaTralla"))
                .Expect<object[]>("[someList.where(y:(y.someInt mod 3) eq 1)] as this",
                                  _this => ((IEnumerable<FooBar>)_this[0]).Any(y => ClientMethod1(y.SomeInt).Bar == "JallaTralla"));
        }


        [Test]
        public void
            Build_FromSplittableExpression_ReferencingSameServerPropertyMultipleTimes_PassedToMethodOnClient_DoesNotRepeatPropertyInSelect
            ()
        {
            BuildAndAssertSplitExpression(x => ClientMethod2(x.Id, x.Bonga, x.Bonga)).Expect<object[]>(
                "[id,bonga] as this",
                _this => ClientMethod2((int)_this[0], (string)_this[1], (string)_this[1]));
        }


        [Test]
        public void Build_FromSplittableExpression_WithClientSideCountSubQuery_PassedToMethodOnClient_RunsQueryOnClient()
        {
            BuildAndAssertSplitExpression(
                x => ClientMethod1(ClientEnumerableFilter(x.SomeList).Count(y => y.SomeDouble > 1.0)))
                .Expect<object[]>("[someList] as this",
                                  _this => ClientMethod1(ClientEnumerableFilter((IList<FooBar>)_this[0]).Count(y => y.SomeDouble > 1.0)));
        }


        [Test]
        public void Build_FromSplittableExpression_WithCountSubQuery_PassedToMethodOnClient_RunsQueryOnServer()
        {
            BuildAndAssertSplitExpression(x => ClientMethod1(x.SomeList.Count()))
                .Expect<object[]>("[count(someList)] as this",
                                  _this => ClientMethod1((int)_this[0]));
        }


        [Test]
        public void Build_FromSplittableExpression_WithCountSubQueryGivenServerUnsupportedLambda_SubQueryIsRunOnClient()
        {
            BuildAndAssertSplitExpression(x => x.SomeList.Any(y => ClientMethod1(y.SomeInt).Bar == "JallaTralla"))
                .Expect<object[]>("[someList] as this",
                                  _this => ((IList<FooBar>)_this[0]).Any(y => ClientMethod1(y.SomeInt).Bar == "JallaTralla"));
        }


        [Test]
        public void Build_FromSplittableExpression_WithPredicatedCountSubQuery_PassedToMethodOnClient_RunsQueryOnServer()
        {
            BuildAndAssertSplitExpression(x => ClientMethod1(x.SomeList.Count(y => y.SomeDouble > 1.0)))
                .Expect<object[]>("[count(someList,y:y.someDouble gt 1.0)] as this",
                                  _this => ClientMethod1((int)_this[0]));
        }


        [Test]
        public void Build_FromSplittableExpression_WithStringConcat_PassedToMethodOnClient_ConcatsOnClient()
        {
            BuildAndAssertSplitExpression(x => ClientMethod2(x.Id, x.Bonga, x.Bonga + x.Jalla))
                .Expect<object[]>("[id,bonga,jalla] as this",
                                  _this => ClientMethod2((int)_this[0], (string)_this[1], string.Concat((string)_this[1], (string)_this[2])));
        }


        [Test]
        public void Build_FromSplittableNewExpression_RunsNewPartOnClient()
        {
            BuildAndAssertSplitExpression(x => new SelectedClientSideClass(x.Jalla))
                .Expect<object[]>("[jalla] as this", _this => new SelectedClientSideClass((string)_this[0]));
        }


        [Test]
        public void Build_FromSplittableNewExpressionWithMemberInitializers_RunsNewPartOnClient()
        {
            BuildAndAssertSplitExpression(x => new SelectedClientSideClass(x.Jalla) { Foo = x.Bonga })
                .Expect<object[]>("[jalla,bonga] as this", _this => new SelectedClientSideClass((string)_this[0]) { Foo = (string)_this[1] });
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


        private static PomonaExtendedExpression Build<T>(Expression<Func<TestResource, T>> expr)
        {
            return (PomonaExtendedExpression)expr.Visit<ClientServerSplittingSelectBuilder>();
        }


        private IEnumerable<T> ClientEnumerableFilter<T>(IEnumerable<T> source)
        {
            throw new NotImplementedException("Only here as example for test.");
        }


        private SelectedClientSideClass ClientMethod1(int theId)
        {
            throw new NotImplementedException("Only placed here as example.");
        }


        private SelectedClientSideClass ClientMethod2(int theId, string foo, string bar)
        {
            throw new NotImplementedException("Only placed here as example.");
        }


        private static Expression<Func<TestResource, T>> Lambda<T>(Expression<Func<TestResource, T>> expr)
        {
            return expr;
        }


        public class SelectedClientSideClass
        {
            public SelectedClientSideClass(string bar)
            {
                Bar = bar;
            }


            public string Bar { get; }

            public string Foo { get; set; }
        }

        protected class SplitExpressionAsserter<TRet>
        {
            private readonly Expression<Func<TestResource, TRet>> expr;


            public SplitExpressionAsserter(Expression<Func<TestResource, TRet>> expr)
            {
                if (expr == null)
                    throw new ArgumentNullException(nameof(expr));
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
    }
}

