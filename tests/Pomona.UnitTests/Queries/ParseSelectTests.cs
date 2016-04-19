#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.CodeGen;
using Pomona.Queries;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class ParseSelectTests : QueryExpressionParserTestsBase
    {
        [Test]
        public void ParseSelectList_WithExplicitNames_UsingGeneratedAnonymousType_ReturnsCorrectExpression()
        {
            ParseAndAssertSelect("Friend as Venn,Guid as LongRandomNumber",
                                 _this => new { Venn = _this.Friend, LongRandomNumber = _this.Guid });
        }


        [Test]
        public void ParseSelectList_WithExplicitNames_UsingStringObjectDictionary_ReturnsCorrectExpression()
        {
            ParseAndAssertSelect("Friend as Venn,Guid as LongRandomNumber",
                                 _this => new Dictionary<string, object> { { "Venn", _this.Friend }, { "LongRandomNumber", _this.Guid } },
                                 false);
            //ParseAndAssertSelect("Friend as Venn,Guid as LongRandomNumber",
            //                     _this => new {Venn = _this.Friend, LongRandomNumber = _this.Guid});
        }


        [Test]
        public void ParseSelectList_WithImplicitName_UsingGeneratedAnonymousType_ReturnsCorrectExpression()
        {
            ParseAndAssertSelect("Friend.Parent.Number,Parent", _this => new { _this.Friend.Parent.Number, _this.Parent });
        }


        [Test]
        public void ParseSelectList_WithImplicitName_UsingStringObjectDictionary_ReturnsCorrectExpression()
        {
            this.parser = new QueryExpressionParser(new QueryTypeResolver(this.typeMapper));
            ParseAndAssertSelect("Friend.Parent.Number,Parent",
                                 _this =>
                                     new Dictionary<string, object> { { "Number", _this.Friend.Parent.Number }, { "Parent", _this.Parent } },
                                 false);
        }


        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            // This is needed to get parser to use same anonymous types as those in expected expressions.
            AnonymousTypeBuilder.ScanAssemblyForExistingAnonymousTypes(GetType().Assembly);
        }


        protected void ParseAndAssertSelect<TRet>(string selectExpr, Expression<Func<Dummy, TRet>> expected, bool useAnonymousType = true)
        {
            var actual = this.parser.ParseSelectList(typeof(Dummy), selectExpr, useAnonymousType);
            actual.AssertEquals(expected);
        }
    }
}