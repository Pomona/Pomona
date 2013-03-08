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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Pomona.Common;
using Pomona.Queries;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class ParseFilterTests : QueryExpressionParserTestsBase
    {
        private T AssertIsConstant<T>(Expression expr)
        {
            var constExpr = AssertCast<ConstantExpression>(expr);
            Assert.That(constExpr.Type, Is.EqualTo(typeof (T)));
            return (T) constExpr.Value;
        }


        private T AssertCast<T>(object obj)
            where T : class
        {
            var objAsT = obj as T;
            if (objAsT == null)
                Assert.Fail("Failed to cast object to " + typeof (T).Name + ", was of type" + obj.GetType().Name);
            return objAsT;
        }


        private void AssertExpressionEquals<T, TReturn>(
            Expression<Func<T, TReturn>> actual, Expression<Func<T, TReturn>> expected)
        {
            AssertExpressionEquals(actual, (Expression) expected);
        }


        private void AssertExpressionEquals(Expression actual, Expression expected)
        {
            actual.AssertEquals(expected);
        }


        public void Parse_CastExpression_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("cast()");
        }


        public class EvaluateClosureMemberVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression.NodeType == ExpressionType.Constant)
                {
                    var target = ((ConstantExpression) node.Expression).Value;

                    var propInfo = node.Member as PropertyInfo;
                    if (propInfo != null)
                        return Expression.Constant(propInfo.GetValue(target, null), propInfo.PropertyType);
                    var fieldInfo = node.Member as FieldInfo;
                    if (fieldInfo != null)
                        return Expression.Constant(fieldInfo.GetValue(target), fieldInfo.FieldType);
                }
                return base.VisitMember(node);
            }
        }


        [Test]
        public void Parse_AnyExpressionWithLambda_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("any(Children,x:x.Number eq 5 and any(x.SomeStrings,y:y eq x.Text))");
            AssertExpressionEquals(
                expr, _this => _this.Children.Any(x => x.Number == 5 && x.SomeStrings.Any(y => y == x.Text)));
        }


        [Test]
        public void Parse_ArrayWithExpressionOfSimpleValuesContains_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("Number in [3,Number,4]");

            // TODO: Array of constants should be a ConstantExpression maybe? [KNS]
            AssertExpressionEquals(expr, _this => (new[] {3, _this.Number, 4}).Contains(_this.Number));
        }

        [Test]
        public void Parse_CastToInt32_CreatesCorrectExpression()
        {
            // TODO: Also create system tests for this!
            var expr = parser.Parse<Dummy>("cast(precise,'Int32') eq number");
            AssertExpressionEquals(expr, _this => (int) _this.Precise == _this.Number);
        }

        [Test]
        public void Parse_ComparisonBetweenObjectPropertyAndNullableConstant_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("unknownProperty gt 44");
            AssertExpressionEquals(expr, _this => _this.UnknownProperty as int? > 44);
            var expr2 = parser.Parse<Dummy>("44 lt unknownProperty");
            AssertExpressionEquals(expr2, _this => 44 < (_this.UnknownProperty as int?));
            var expr3 = parser.Parse<Dummy>("44 sub unknownProperty lt unknownProperty add 447");
            AssertExpressionEquals(expr3,
                                   _this => 44 - (_this.UnknownProperty as int?) < (_this.UnknownProperty as int?) + 447);
        }


        [Test]
        public void Parse_ConstantArrayOfSimpleValuesContains_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("Number in [3,2,4]");

            // TODO: Array of constants should be a ConstantExpression maybe? [KNS]
            var ints = new[] {3, 2, 4};
            var evaluateClosureVisitor = new EvaluateClosureMemberVisitor();
            Expression<Func<Dummy, bool>> expected = _this => ints.Contains(_this.Number);
            // We need to evaluate closure display class field accesses to get the same expression
            expected = (Expression<Func<Dummy, bool>>) evaluateClosureVisitor.Visit(expected);
            AssertExpressionEquals(expr, expected);
        }


        [Test]
        public void Parse_DateTimeConstant_CreatesCorrectExpression()
        {
            var dateTimeString = "2000-12-12T12:00";
            var expectedTime = DateTime.Parse(dateTimeString);
            var expr = parser.Parse<Dummy>(string.Format("Time eq datetime'{0}'", dateTimeString));
            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            var leftTimeConstant = AssertIsConstant<DateTime>(binExpr.Right);
            Assert.That(leftTimeConstant, Is.EqualTo(expectedTime));
        }

        [Test]
        public void Parse_DictAccess_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("attributes['foo'] eq 'bar'");
            AssertExpressionEquals(expr, _this => _this.Attributes["foo"] == "bar");
        }

        [Test]
        public void Parse_EscapedPropertyNameSameAsOperator_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("@and and @and");
            AssertExpressionEquals(expr, _this => _this.and && _this.and);
        }

        [Test]
        public void Parse_ExpressionWithGrammarError_ThrowsExceptionWithUsefulMessage()
        {
            var exception = Assert.Throws<QueryParseException>(() => parser.Parse<Dummy>("name eo 'blah'"));
            Assert.That(exception.Message.Replace("\r", ""), Is.StringContaining(
                @"Error on line 1 character 5 of query:
     |/
name eo 'blah'".Replace("\r", "")));
        }


        [Test]
        public void Parse_GuidConstant_CreatesCorrectExpression()
        {
            var guid = Guid.NewGuid();
            var expr = parser.Parse<Dummy>(string.Format("Guid eq guid'{0}'", guid));
            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            var leftGuidConstant = AssertIsConstant<Guid>(binExpr.Right);
            Assert.That(leftGuidConstant, Is.EqualTo(guid));
        }

        [Test]
        public void Parse_MultiLineExpressionWithGrammarError_ThrowsExceptionWithUsefulMessage()
        {
            var exception = Assert.Throws<QueryParseException>(() => parser.Parse<Dummy>("name  \r\n  eo 'blah'"));
            Assert.That(exception.Message.Replace("\r", ""), Is.StringContaining(
                @"Error on line 2 character 2 of query:
  |/
  eo 'blah'".Replace("\r", "")));
        }

        [Test]
        public void Parse_NotEqualOperator_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("Number ne 8");
            AssertExpressionEquals(expr, _this => _this.Number != 8);
        }

        [Test]
        public void Parse_NotOperator_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("not OnOrOff");
            AssertExpressionEquals(expr, _this => !_this.OnOrOff);
        }


        [Test]
        public void Parse_Null_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("Text eq null");
            AssertExpressionEquals(expr, _this => _this.Text == null);
        }


        [Test]
        public void Parse_PropertyEqualsIntegerAddedToInteger_ReturnsCorrectResult()
        {
            var lambda = parser.Parse<Dummy>("Number eq 2 add 3");
            var binExpr = AssertCast<BinaryExpression>(lambda.Body);
            AssertCast<MemberExpression>(binExpr.Left);
            var addExpr = AssertCast<BinaryExpression>(binExpr.Right);
            Assert.That(addExpr.NodeType, Is.EqualTo(ExpressionType.Add));
            Assert.That(addExpr.Type, Is.EqualTo(typeof (int)));
            var leftAddInt = AssertIsConstant<int>(addExpr.Left);
            Assert.That(leftAddInt, Is.EqualTo(2));
            var rightAddInt = AssertIsConstant<int>(addExpr.Right);
            Assert.That(rightAddInt, Is.EqualTo(3));
        }


        [Test]
        public void Parse_PropertyEqualsStringExpression_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("Text eq 'Jalla'");

            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            AssertCast<MemberExpression>(binExpr.Left);
            var rightExprString = AssertIsConstant<string>(binExpr.Right);
            Assert.That(rightExprString, Is.EqualTo("Jalla"));
        }


        [Test]
        public void Parse_PropertyEqualsStringExpression_ReturnsCorrectResult()
        {
            var lambda = parser.Parse<Dummy>("Text eq 'Jalla'").Compile();

            var jallaDummy = new Dummy {Text = "Jalla"};
            var negativeDummy = new Dummy {Text = "Boooo"};

            Assert.That(lambda(jallaDummy), Is.True);
            Assert.That(lambda(negativeDummy), Is.False);
        }


        [Test]
        public void Parse_PropertyEqualsStringOrPropertyEqualString_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("Text eq 'Jalla' or Text eq 'Mohahaa'");
            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            Assert.That(binExpr.NodeType, Is.EqualTo(ExpressionType.OrElse));
        }

        [Test]
        public void Parse_RecursiveDotting_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("parent.parent.friend.text eq 'whoot'");
            AssertExpressionEquals(expr, _this => _this.Parent.Parent.Friend.Text == "whoot");
        }

        [Test]
        public void Parse_ShortHandComparisonBetweenObjectDictMemberAndInteger_CreatesCorrectExpression()
        {
            var expr2 = parser.Parse<Dummy>("5 lt (objectAttributes.jalla add 55)");
            AssertExpressionEquals(expr2, _this => 5 < (_this.ObjectAttributes.SafeGet("jalla") as int?) + 55);
            var expr = parser.Parse<Dummy>("objectAttributes.jalla gt 5");
            AssertExpressionEquals(expr, _this => _this.ObjectAttributes.SafeGet("jalla") as int? > 5);
        }

        [Test]
        public void Parse_ShortHandComparisonBetweenObjectDictMemberAndString_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("objectAttributes.jalla eq 'bloborob'");
            AssertExpressionEquals(expr, _this => _this.ObjectAttributes.SafeGet("jalla") as string == "bloborob");
        }


        [Test]
        public void Parse_SubSelectExpressionChained_CreatesCorrectExpression()
        {
            var exprChained = parser.Parse<Dummy>("sum(Children.select(x:x.Number)) gt 10");
            AssertExpressionEquals(exprChained, _this => _this.Children.Select(x => x.Number).Sum() > 10);
        }

        [Test]
        public void Parse_SubSelectExpression_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("sum(select(Children,x:x.Number)) gt 10");
            AssertExpressionEquals(expr, _this => _this.Children.Select(x => x.Number).Sum() > 10);
        }


        [Test]
        public void Parse_ThreeTimesOr_CreatesCorrectExpression()
        {
            Expression<Func<Dummy, bool>> expected =
                _this => _this.Number == 4 || _this.Number == 66 || _this.Number == 2;
            var expr = parser.Parse<Dummy>("Number eq 4 or Number eq 66 or Number eq 2");

            AssertExpressionEquals(expr, expected);
        }


        [Test]
        public void Parse_ToStringWithExtensionMethodCallStyle_CreatesCorrectExpression()
        {
            var expr = parser.Parse<Dummy>("text.tolower().substring(0,3) eq 'whoot'");
            AssertExpressionEquals(expr, _this => _this.Text.ToLower().Substring(0, 3) == "whoot");

            var expr2 = parser.Parse<Dummy>("text.substring(2,5).substringof(parent.text)");
            // Note that Contains is the .NET "inverted" version of substringof,
            // so in C# this looks different (which is correct).
            AssertExpressionEquals(expr2, _this => _this.Parent.Text.Contains(_this.Text.Substring(2, 5)));
        }


        [Test]
        public void Parse_WithProperty_ResolvesToCorrectProperty()
        {
            var expr = parser.Parse<Dummy>("Text eq 'Jalla'");

            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            var memberExpr = AssertCast<MemberExpression>(binExpr.Left);
            var prop = memberExpr.Member as PropertyInfo;
            Assert.That(prop, Is.Not.Null, "Member expression expected to be of type PropertyInfo");
            Assert.That(prop.DeclaringType, Is.EqualTo(typeof (Dummy)));
            Assert.That(prop.Name, Is.EqualTo("Text"));
        }
    }
}