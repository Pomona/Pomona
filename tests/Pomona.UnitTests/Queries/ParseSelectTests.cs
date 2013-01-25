using System;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Pomona.CodeGen;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class ParseSelectTests : QueryExpressionParserTestsBase
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // This is needed to get parser to use same anonymous types as those in expected expressions.
            AnonymousTypeBuilder.ScanAssemblyForExistingAnonymousTypes(this.GetType().Assembly);
        }

        private void ParseAndAssert<TRet>(string selectExpr, Expression<Func<Dummy, TRet>> expected)
        {
            var actual = parser.ParseSelectList(typeof (Dummy), selectExpr);
            actual.AssertEquals(expected);
        }

        [Test]
        public void ParseSelectList_WithImplicitName_ReturnsCorrectExpression()
        {
            ParseAndAssert("Friend.Parent.Number,Parent", _this => new {_this.Friend.Parent.Number, _this.Parent});
        }
        [Test]
        public void ParseSelectList_WithExplicitNames_ReturnsCorrectExpression()
        {
            ParseAndAssert("Friend as Venn,Guid as LongRandomNumber", _this => new { Venn = _this.Friend, LongRandomNumber = _this.Guid });
        }
    }
}