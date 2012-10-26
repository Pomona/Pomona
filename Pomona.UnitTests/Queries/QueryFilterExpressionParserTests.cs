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
using System.Linq.Expressions;
using System.Reflection;

using NUnit.Framework;

using Pomona.Queries;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class QueryFilterExpressionParserTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.parser = new QueryFilterExpressionParser(new SimpleQueryPropertyResolver());
        }

        #endregion

        private QueryFilterExpressionParser parser;

        public class SimpleQueryPropertyResolver : IQueryTypeResolver
        {
            #region IQueryPropertyResolver Members

            public Type Resolve(string typeName)
            {
                throw new NotImplementedException();
            }


            public Expression Resolve<T>(Expression rootInstance, string propertyPath)
            {
                return Expression.Property(rootInstance, typeof(T).GetProperty(propertyPath));
            }

            #endregion
        }

        public class Dummy
        {
            public Guid Guid { get; set; }
            public int Number { get; set; }
            public string Text { get; set; }
            public DateTime Time { get; set; }
        }


        private T AssertIsConstant<T>(Expression expr)
        {
            var constExpr = AssertCast<ConstantExpression>(expr);
            Assert.That(constExpr.Type, Is.EqualTo(typeof(T)));
            return (T)constExpr.Value;
        }


        private T AssertCast<T>(object obj)
            where T : class
        {
            var objAsT = obj as T;
            if (objAsT == null)
                Assert.Fail("Failed to cast object to " + typeof(T).Name + ", was of type" + obj.GetType().Name);
            return objAsT;
        }


        private void AssertExpressionEquals(Expression expected, Expression actual)
        {
            try
            {
                if (actual.NodeType != expected.NodeType)
                    Assert.Fail("Expected nodetype " + expected.NodeType + " got nodetype " + actual.NodeType);

                var actualLambdaExpr = actual as LambdaExpression;
                if (actualLambdaExpr != null)
                {
                    var expectedLambdaExpr = (LambdaExpression)expected;
                    AssertExpressionEquals(expectedLambdaExpr.Body, actualLambdaExpr.Body);
                    return;
                }

                var actualBinExpr = actual as BinaryExpression;
                if (actualBinExpr != null)
                {
                    var expectedBinExpr = (BinaryExpression)expected;

                    AssertExpressionEquals(expectedBinExpr.Left, actualBinExpr.Left);
                    AssertExpressionEquals(expectedBinExpr.Right, actualBinExpr.Right);
                    return;
                }

                var actualConstExpr = actual as ConstantExpression;
                if (actualConstExpr != null)
                {
                    var expectedConstExpr = (ConstantExpression)expected;
                    if (actualConstExpr.Type != expectedConstExpr.Type)
                    {
                        Assert.Fail(
                            "Got wrong type for constant expression, expected " + expectedConstExpr.Type +
                            ", but got " + actualConstExpr.Type);
                    }

                    if (!actualConstExpr.Value.Equals(expectedConstExpr.Value))
                        Assert.Fail("Constant expression was not of expected value " + expectedConstExpr.Value);
                    return;
                }

                var actualMemberExpr = actual as MemberExpression;
                if (actualMemberExpr != null)
                {
                    var expectedMemberExpr = (MemberExpression)expected;
                    if (actualMemberExpr.Member != expectedMemberExpr.Member)
                        Assert.Fail("Wrong method on memberexpression when comparing expressions..");
                    return;
                }

                throw new NotImplementedException("Don't know how to compare expression node" + actual);
            }
            catch
            {
                Console.WriteLine("Expected expression: " + expected + "\r\nActual expression:" + actual);
            }
        }


        [Test]
        public void Parse_DateTimeConstant_CreatesCorrectExpression()
        {
            var dateTimeString = "2000-12-12T12:00";
            var expectedTime = DateTime.Parse(dateTimeString);
            var expr = this.parser.Parse<Dummy>(string.Format("Time eq datetime'{0}'", dateTimeString));
            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            var leftTimeConstant = AssertIsConstant<DateTime>(binExpr.Right);
            Assert.That(leftTimeConstant, Is.EqualTo(expectedTime));
        }


        [Test]
        public void Parse_GuidConstant_CreatesCorrectExpression()
        {
            var guid = Guid.NewGuid();
            var expr = this.parser.Parse<Dummy>(string.Format("Guid eq guid'{0}'", guid));
            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            var leftGuidConstant = AssertIsConstant<Guid>(binExpr.Right);
            Assert.That(leftGuidConstant, Is.EqualTo(guid));
        }


        [Test]
        public void Parse_PropertyEqualsIntegerAddedToInteger_ReturnsCorrectResult()
        {
            var lambda = this.parser.Parse<Dummy>("Number eq 2 add 3");
            var binExpr = AssertCast<BinaryExpression>(lambda.Body);
            AssertCast<MemberExpression>(binExpr.Left);
            var addExpr = AssertCast<BinaryExpression>(binExpr.Right);
            Assert.That(addExpr.NodeType, Is.EqualTo(ExpressionType.Add));
            Assert.That(addExpr.Type, Is.EqualTo(typeof(int)));
            var leftAddInt = AssertIsConstant<int>(addExpr.Left);
            Assert.That(leftAddInt, Is.EqualTo(2));
            var rightAddInt = AssertIsConstant<int>(addExpr.Right);
            Assert.That(rightAddInt, Is.EqualTo(3));
        }


        [Test]
        public void Parse_PropertyEqualsStringExpression_CreatesCorrectExpression()
        {
            var expr = this.parser.Parse<Dummy>("Text eq 'Jalla'");

            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            AssertCast<MemberExpression>(binExpr.Left);
            var rightExprString = AssertIsConstant<string>(binExpr.Right);
            Assert.That(rightExprString, Is.EqualTo("Jalla"));
        }


        [Test]
        public void Parse_PropertyEqualsStringExpression_ReturnsCorrectResult()
        {
            var lambda = this.parser.Parse<Dummy>("Text eq 'Jalla'").Compile();

            var jallaDummy = new Dummy() { Text = "Jalla" };
            var negativeDummy = new Dummy() { Text = "Boooo" };

            Assert.That(lambda(jallaDummy), Is.True);
            Assert.That(lambda(negativeDummy), Is.False);
        }


        [Test]
        public void Parse_PropertyEqualsStringOrPropertyEqualString_CreatesCorrectExpression()
        {
            var expr = this.parser.Parse<Dummy>("Text eq 'Jalla' or Text eq 'Mohahaa'");
            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            Assert.That(binExpr.NodeType, Is.EqualTo(ExpressionType.OrElse));
        }


        [Test]
        public void Parse_StringConstantAlone_ThrowsArgumentException()
        {
            Assert.That(() => this.parser.Parse<Dummy>("'blah'"), Throws.ArgumentException);
        }


        [Test]
        public void Parse_ThreeTimesOr_CreatesCorrectExpression()
        {
            Expression<Func<Dummy, bool>> expected = x => x.Number == 4 || x.Number == 66 || x.Number == 2;
            var expr = this.parser.Parse<Dummy>("Number eq 4 or Number eq 66 or Number eq 2");

            AssertExpressionEquals(expected, expr);
        }


        [Test]
        public void Parse_WithProperty_ResolvesToCorrectProperty()
        {
            var expr = this.parser.Parse<Dummy>("Text eq 'Jalla'");

            var binExpr = AssertCast<BinaryExpression>(expr.Body);
            var memberExpr = AssertCast<MemberExpression>(binExpr.Left);
            var prop = memberExpr.Member as PropertyInfo;
            Assert.That(prop, Is.Not.Null, "Member expression expected to be of type PropertyInfo");
            Assert.That(prop.DeclaringType, Is.EqualTo(typeof(Dummy)));
            Assert.That(prop.Name, Is.EqualTo("Text"));
        }
    }
}