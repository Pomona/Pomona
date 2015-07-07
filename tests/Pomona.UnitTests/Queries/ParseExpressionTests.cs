#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Linq;

using NUnit.Framework;

using Pomona.Common;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class ParseExpressionTests : QueryExpressionParserTestsBase
    {
        [Test]
        public void Parse_Conditional_IfTrueYieldsNonNullableInt_IfFalseYieldsNullableInt_CreatesCorrectExpression()
        {
            ParseAndAssert("iif(onOrOff,44433,nullableNumber)", _this => _this.OnOrOff ? 44433 : _this.NullableNumber);
        }


        [Test]
        public void Parse_Conditional_IfTrueYieldsNullableBool_IfFalseYieldsNonNullableBool_CreatesCorrectExpression()
        {
            ParseAndAssert("iif(onOrOff,nullableBool,true)", _this => _this.OnOrOff ? _this.NullableBool : true);
        }


        [Test]
        public void Parse_Conditional_IfTrueYieldsNullableInt_IfFalseYieldsNonNullableInt_CreatesCorrectExpression()
        {
            ParseAndAssert("iif(onOrOff,nullableNumber,44433)", _this => _this.OnOrOff ? _this.NullableNumber : 44433);
        }


        [Test]
        public void Parse_InExpressionWithNullOnLeftSide_CreatesCorrectExpression()
        {
            int[] numbers = null;
            ParseAndAssert("number in null", _this => numbers.Contains(_this.Number));
        }


        [Test]
        public void Parse_NullableBoolNotEqualsTrue_CreatesCorrectExpression()
        {
            ParseAndAssert("nullableBool ne true", _this => _this.NullableBool != true);
        }


        [Test]
        public void Parse_NullableInt64EqualsConstant_CreatesCorrectExpression()
        {
            ParseAndAssert("nullableInt64 eq 42L", _this => _this.NullableInt64 == 42L);
        }


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
            ParseAndAssert("convert(text,t'Int32')", _this => (int)Convert.ChangeType(_this.Text, typeof(int)));
        }


        [Test]
        public void ParseEnumComparison_ReturnsCorrectExpression()
        {
            ParseAndAssert("anEnumValue eq 'Moo'", _this => _this.AnEnumValue == TestEnum.Moo);
        }


        [Test]
        public void ParseEnumNotEqual_ReturnsCorrectExpression()
        {
            ParseAndAssert("anEnumValue ne 'Moo'", _this => _this.AnEnumValue != TestEnum.Moo);
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
        public void ParseLiteralNegativeInt_ReturnsCorrectExpression()
        {
            ParseAndAssert("-1234'", _this => -1234);
        }


        [Test]
        public void ParseLiteralPositiveInt_ReturnsCorrectExpression()
        {
            ParseAndAssert("1234'", _this => 1234);
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
        public void ParseNotEquals_ReturnsCorrectExpression()
        {
            ParseAndAssert("number ne 3", _this => _this.Number != 3);
        }


        [Test]
        public void ParseNullableEnumComparison_ReturnsCorrectExpression()
        {
            ParseAndAssert("NullableEnum eq 'Moo'", _this => _this.NullableEnum == TestEnum.Moo);
        }


        [Test]
        public void ParseNullableEnumHasValue_ReturnsCorrectExpression()
        {
            ParseAndAssert("nullableEnum.hasValue()", _this => _this.NullableEnum.HasValue);
        }


        [Test]
        public void ParseNumberInEmptyCollection_CreatesCorrectExpression()
        {
            var numbers = new int[] { };
            ParseAndAssert("number in []", _this => numbers.Contains(_this.Number));
        }


        [Test]
        public void ParseObjectAttributesIn_CreatesCorrectExpression()
        {
            object[] objects = new object[] { 12, 43, 66 };
            ParseAndAssert("objectAttributes.jalla in [12,43,66]",
                           _this => objects.Contains(_this.ObjectAttributes.SafeGet("jalla")));
        }


        [Test]
        public void ParseObjectAttributesMemberSafeGet_CreatesCorrectExpression()
        {
            ParseAndAssert("objectAttributes.jalla", _this => _this.ObjectAttributes.SafeGet("jalla"));
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
        public void ParseSingle_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.single()", _this => _this.Children.Single());
        }


        [Test]
        public void ParseSingleOrDefault_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.singledefault()", _this => _this.Children.SingleOrDefault());
        }


        [Test]
        public void ParseSingleOrDefaultWithPredicate_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.singledefault(x:x.number eq 4)",
                           _this => _this.Children.SingleOrDefault(x => x.Number == 4));
        }


        [Test]
        public void ParseSingleWithPredicate_ReturnsCorrectExpression()
        {
            ParseAndAssert("children.single(x:x.number eq 4)",
                           _this => _this.Children.Single(x => x.Number == 4));
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
            ParseAndAssert("t'Int32'", _this => typeof(int));
        }


        [Test]
        public void ParseTypeNameLiteralExpression_WithNullableInt32_ReturnsCorrectExpression()
        {
            ParseAndAssert("t'Int32?'", _this => typeof(int?));
        }


        [Test]
        public void ParseValuePropertyOfNullable_ReturnsCorrectString()
        {
            ParseAndAssert("nullableNumber.value()", _this => _this.NullableNumber.Value);
        }
    }
}