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
        public void ParseAverageOfDecimalEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfDecimals.average()", _this => _this.ListOfDecimals.Average());
        }

        [Test]
        public void ParseAverageOfDecimalsWithSelector_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.average(y:y.someDecimal)", _this => _this.Children.Average(y => y.SomeDecimal));
        }

        [Test]
        public void ParseAverageOfDoubleEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfDoubles.average()", _this => _this.ListOfDoubles.Average());
        }

        [Test]
        public void ParseAverageOfDoublesWithSelector_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.average(y:y.precise)", _this => _this.Children.Average(y => y.Precise));
        }

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
        public void ParseNotEquals_ReturnsCorrectExpression()
        {
            ParseAndAssert("number ne 3", _this => _this.Number != 3);
        }

        [Test]
        public void ParseMaxOfEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfInts.max()", _this => _this.ListOfInts.Max());
            ParseAndAssert("listOfDoubles.max()", _this => _this.ListOfDoubles.Max());
            ParseAndAssert("listOfDecimals.max()", _this => _this.ListOfDecimals.Max());
            ParseAndAssert("listOfFloats.max()", _this => _this.ListOfFloats.Max());
        }

        [Test]
        public void ParseMaxWithSelector_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfInts.max(y: y)", _this => _this.ListOfInts.Max(y => y));
            ParseAndAssert("listOfDoubles.max(y: y)", _this => _this.ListOfDoubles.Max(y => y));
            ParseAndAssert("listOfDecimals.max(y: y)", _this => _this.ListOfDecimals.Max(y => y));
            ParseAndAssert("listOfFloats.max(y: y)", _this => _this.ListOfFloats.Max(y => y));
        }

        [Test]
        public void ParseMinOfIntEnumerable_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfInts.min()", _this => _this.ListOfInts.Min());
            ParseAndAssert("listOfDoubles.min()", _this => _this.ListOfDoubles.Min());
            ParseAndAssert("listOfDecimals.min()", _this => _this.ListOfDecimals.Min());
            ParseAndAssert("listOfFloats.min()", _this => _this.ListOfFloats.Min());
        }

        [Test]
        public void ParseMinWithSelector_ReturnsCorrectExpression()
        {
            ParseAndAssert("listOfInts.min(y: y)", _this => _this.ListOfInts.Min(y => y));
            ParseAndAssert("listOfDoubles.min(y: y)", _this => _this.ListOfDoubles.Min(y => y));
            ParseAndAssert("listOfDecimals.min(y: y)", _this => _this.ListOfDecimals.Min(y => y));
            ParseAndAssert("listOfFloats.min(y: y)", _this => _this.ListOfFloats.Min(y => y));
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
        public void ParseSingleOrDefaultWithPredicate_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.singledefault(x:x.number eq 4)",
                           _this => _this.Children.SingleOrDefault(x => x.Number == 4));
        }


        [Test]
        public void ParseSingleOrDefault_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.singledefault()", _this => _this.Children.SingleOrDefault());
        }

        [Test]
        public void ParseSingleWithPredicate_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.single(x:x.number eq 4)",
                           _this => _this.Children.Single(x => x.Number == 4));
        }

        [Test]
        public void ParseSingle_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.single()", _this => _this.Children.Single());
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
        public void ParseEnumNotEqual_ReturnsCorrectExpression()
        {
            ParseAndAssert("anEnumValue ne 'Moo'", _this => _this.AnEnumValue != TestEnum.Moo);
        }

        [Test]
        public void ParseEnumComparison_ReturnsCorrectExpression()
        {
            ParseAndAssert("anEnumValue eq 'Moo'", _this => _this.AnEnumValue == TestEnum.Moo);
        }

        [Test]
        public void ParseNullableEnumComparison_ReturnsCorrectExpression()
        {
            ParseAndAssert("NullableEnum eq 'Moo'", _this => _this.NullableEnum == TestEnum.Moo);
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