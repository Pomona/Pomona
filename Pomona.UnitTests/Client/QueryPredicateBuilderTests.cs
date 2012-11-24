#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

using Pomona.Client;

namespace Pomona.UnitTests.Client
{
    [TestFixture]
    public class QueryPredicateBuilderTests
    {
        public class FooBar : IClientResource
        {
            public string SomeString { get; set; }
            public IList<TestResource> TestResources { get; set; }
        }

        public class TestResource : IClientResource
        {
            public IDictionary<string, string> Attributes { get; set; }
            public DateTime Birthday { get; set; }
            public string Bonga { get; set; }
            public dynamic Boo { get; set; }
            public decimal CashAmount { get; set; }
            public Guid Guid { get; set; }
            public string Jalla { get; set; }
            public float LessPrecise { get; set; }
            public double Precise { get; set; }
            public IList<FooBar> SomeList { get; set; }
            public TimeSpan TimeSpan { get; set; }
        }

        public class Container
        {
            public string Junk { get; set; }
        }


        private void AssertBuild<T>(Expression<Func<TestResource, T>> predicate, string expected)
        {
            var builder = QueryPredicateBuilder.Create<TestResource, T>(predicate);
            var queryString = builder.ToString();
            Console.WriteLine("Transformed \"" + predicate + "\" TO \"" + queryString + "\"");
            Assert.That(queryString, Is.EqualTo(expected));
        }


        [Test]
        public void BuildAnyLambdaExpression_ReturnsCorrectString()
        {
            AssertBuild(x => x.SomeList.Any(y => y.SomeString == "lalala"), "someList.any(y:y.someString eq 'lalala')");
        }


        [Test]
        public void BuildConcat_ReturnsCorrectString()
        {
            AssertBuild(x => string.Concat(x.Jalla, "boo"), "concat(jalla,'boo')");
            AssertBuild(x => x.Jalla + "boo", "concat(jalla,'boo')");
            AssertBuild(x => x.Jalla + "boo" + "faa" + "foo", "concat(concat(concat(jalla,'boo'),'faa'),'foo')");
        }


        [Test]
        public void BuildConstantExpression_UsingNestedClosureAccess_ReturnsConstant()
        {
            var container = new Container() { Junk = "Kirk" };
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Jalla == container.Junk);
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("jalla eq 'Kirk'"));
        }


        [Test]
        public void BuildCountOfList_ReturnsCorrectString()
        {
            AssertBuild(x => x.SomeList.Count == 4, "count(someList) eq 4");
        }


        [Test]
        public void BuildDateComponentExtractExpressions_ReturnsCorrectString()
        {
            AssertBuild(x => x.Birthday.Year == 2012, "year(birthday) eq 2012");
            AssertBuild(x => x.Birthday.Month == 10, "month(birthday) eq 10");
            AssertBuild(x => x.Birthday.Day == 15, "day(birthday) eq 15");
            AssertBuild(x => x.Birthday.Hour == 11, "hour(birthday) eq 11");
            AssertBuild(x => x.Birthday.Minute == 33, "minute(birthday) eq 33");
            AssertBuild(x => x.Birthday.Second == 44, "second(birthday) eq 44");
        }


        [Test]
        public void BuildDateTimeUtc_ReturnsCorrectString()
        {
            var dt = new DateTime(2012, 10, 22, 5, 32, 45, DateTimeKind.Utc);
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Birthday == dt);
            var queryString = builder.ToString();

            Assert.That(queryString, Is.EqualTo("birthday eq datetime'2012-10-22T05:32:45Z'"));
        }


        [Test]
        public void BuildDateTime_ReturnsCorrectString()
        {
            var dt = new DateTime(2012, 10, 22, 5, 32, 45, DateTimeKind.Local);
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Birthday == dt);
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("birthday eq datetime'2012-10-22T05:32:45'"));
        }


        [Test]
        public void BuildDecimal_ReturnsCorrectString()
        {
            AssertBuild(x => 10.25m, "10.25m");
        }


        [Test]
        public void BuildDouble_ReturnsCorrectString()
        {
            AssertBuild(x => 10.25, "10.25");
        }


        [Test]
        public void BuildEndsWithExpression_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla.EndsWith("boja"), "endswith(jalla,'boja')");
        }


        [Test]
        public void BuildEqualExpression_ReturnsCorrectString()
        {
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Jalla == "What");
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("jalla eq 'What'"));
        }


        [Test]
        public void BuildFalse_ReturnsCorrectString()
        {
            AssertBuild(x => false, "false");
        }


        [Test]
        public void BuildFloat_ReturnsCorrectString()
        {
            AssertBuild(x => 10.25f, "10.25f");
            AssertBuild(x => 10f, "10f");
        }


        [Test]
        public void BuildGuidLiteral_ReturnsCorrectString()
        {
            var guidString = "6dd20569-5c87-46f9-8665-9f413d9e7c47";
            var guid = new Guid(guidString);
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Guid == guid);
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo(string.Format("guid eq guid'{0}'", guidString)));
        }


        [Test]
        public void BuildIndexOfExpressionWithCharArg_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla.IndexOf('z') == 2, "indexof(jalla,'z') eq 2");
        }


        [Test]
        public void BuildIndexOfExpression_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla.IndexOf("banana") == 2, "indexof(jalla,'banana') eq 2");
        }


        [Test]
        public void BuildLengthExpression_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla.Length == 1, "length(jalla) eq 1");
        }


        [Test]
        public void BuildNotEquals_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla != "blob", "jalla ne 'blob'");
        }


        [Test]
        public void BuildNull_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla == null, "jalla eq null");
        }


        [Test]
        public void BuildPropEqDouble_ReturnsCorrectString()
        {
            AssertBuild(x => x.Precise == 10.25, "precise eq 10.25");
        }


        [Test]
        public void BuildPropEqFloat_ReturnsCorrectString()
        {
            AssertBuild(x => x.LessPrecise == 10.75f, "lessPrecise eq 10.75f");
        }


        [Test]
        public void BuildPropEqProp_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla == x.Bonga, "jalla eq bonga");
        }


        [Test]
        public void BuildPropEqualsDecimal_ReturnsCorrectString()
        {
            AssertBuild(x => x.CashAmount == 100.10m, "cashAmount eq 100.10m");
        }


        [Test]
        public void BuildRecursiveLambdaExpression_ReturnsCorrectString()
        {
            AssertBuild(
                x => x.SomeList.Any(y => y.SomeString == "lalala" && y.TestResources.Any(z => z.Bonga == y.SomeString)),
                "someList.any(y:(y.someString eq 'lalala') and y.testResources.any(z:z.bonga eq y.someString))");
        }


        [Test]
        public void BuildReplaceExpression_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla.Replace("a", "e") == "crezy", "replace(jalla,'a','e') eq 'crezy'");
        }


        [Test]
        public void BuildStartsWithExpression_ReturnsCorrectString()
        {
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Jalla.StartsWith("Gangnam"));
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("startswith(jalla,'Gangnam')"));
        }


        [Test]
        public void BuildString_EncodesSingleQuoteCorrectly()
        {
            AssertBuild(x => "Banana'Boo", "'Banana''Boo'");
            AssertBuild(x => "'", "''''");
            AssertBuild(x => "''", "''''''");
        }


        [Test]
        public void BuildSubstringExpression_ReturnsCorrectString()
        {
            AssertBuild(x => x.Jalla.Substring(1) == "alla", "substring(jalla,1) eq 'alla'");
            AssertBuild(x => x.Jalla.Substring(1, 2) == "al", "substring(jalla,1,2) eq 'al'");
        }


        [Test]
        public void BuildSubstringOfExpression_ReturnsCorrectString()
        {
            var builder = QueryPredicateBuilder.Create<TestResource>(x => x.Jalla.Contains("cool"));
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("substringof('cool',jalla)"));
        }


        [Test]
        public void BuildTrim_ReturnsCorrectString()
        {
            AssertBuild(x => "   lallala   ".Trim(), "trim('   lallala   ')");
        }


        [Test]
        public void BuildTrue_ReturnsCorrectString()
        {
            AssertBuild(x => true, "true");
        }


        [Test]
        public void GetItemOnDictionary_ReturnsCorrectString()
        {
            AssertBuild(x => x.Attributes["hello world"] == "bob", "attributes['hello world'] eq 'bob'");
        }
    }
}