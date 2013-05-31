// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Linq;
using NUnit.Framework;
using Pomona.Common;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class ParseExpressionTests : QueryExpressionParserTestsBase
    {
        [Test]
        public void ParseConditional_ReturnsCorrectExpression()
        {
            ParseAndAssert("iif(text eq 'lalala', 'yes', 'no')", _this => _this.Text == "lalala" ? "yes" : "no");
        }

        [Test]
        public void ParseConvertToIntExpression_ReturnsCorrectExpression()
        {
            ParseAndAssert("convert(text,t'Int32')", _this => (int) Convert.ChangeType(_this.Text, typeof (int)));
        }

        [Test]
        public void ParseFirstOrDefaultWithPredicate_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.firstdefault(x:x.number eq 4)",
                           _this => _this.Children.FirstOrDefault(x => x.Number == 4));
        }

        [Test]
        public void ParseFirstWithPredicate_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.first(x:x.number eq 4)",
                           _this => _this.Children.First(x => x.Number == 4));
        }

        [Test]
        public void ParseObjectIsOfType_ReturnsCorrectExpression()
        {
            ParseAndAssert("isof(unknownProperty,t'Int32')", _this => _this.UnknownProperty is int);
        }

        [Test]
        public void ParseSafeGetFromObjectDictionaryAsStringExpression_ReturnsCorrectExpression()
        {
            ParseAndAssert("objectAttributes.Hei as t'String'", _this => _this.ObjectAttributes.SafeGet("Hei") as string);
        }

        [Test]
        public void ParseSumOfDecimalEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfDecimals.sum()", _this => _this.ListOfDecimals.Sum());
        }

        [Test]
        public void ParseSumOfDoubleEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfDoubles.sum()", _this => _this.ListOfDoubles.Sum());
        }

        [Test]
        public void ParseSumOfIntEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfInts.sum()", _this => _this.ListOfInts.Sum());
        }

        [Test]
        public void ParseSumWithSelectorOfDecimal_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.sum(x: x.someDecimal)", _this => _this.Children.Sum(x => x.SomeDecimal));
        }

        [Test]
        public void ParseSumWithSelectorOfDouble_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.sum(x: x.precise)", _this => _this.Children.Sum(x => x.Precise));
        }

        [Test]
        public void ParseSumWithSelectorOfInt_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.sum(x: x.number)", _this => _this.Children.Sum(x => x.Number));
        }

        [Test]
        public void ParseTypeNameLiteralExpression_WithInt32_ReturnsCorrectExpression()
        {
            ParseAndAssert("t'Int32'", _this => typeof (int));
        }

        [Test]
        public void ParseTypeNameLiteralExpression_WithNullableInt32_ReturnsCorrectExpression()
        {
            ParseAndAssert("t'Int32?'", _this => typeof (int?));
        }
    }
}