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
using NUnit.Framework;

namespace Pomona.TestHelpers
{
    public static class ExpressionAssertExtensions
    {
        public static void AssertEquals<TDelegate>(this Expression actual, Expression<TDelegate> expected)
        {
            AssertEquals(actual, (Expression) expected);
        }


        public static void AssertEquals(this Expression actual, Expression expected)
        {
            try
            {
                if (expected == null)
                {
                    if (actual != null)
                        Assert.Fail("Expected null expression.");
                    return;
                }

                if (actual.NodeType != expected.NodeType)
                    Assert.Fail("Expected nodetype " + expected.NodeType + " got nodetype " + actual.NodeType);

                var actualLambdaExpr = actual as LambdaExpression;
                if (actualLambdaExpr != null)
                {
                    var expectedLambdaExpr = (LambdaExpression) expected;
                    AssertEquals(actualLambdaExpr.Body, expectedLambdaExpr.Body);
                    return;
                }

                var actualBinExpr = actual as BinaryExpression;
                if (actualBinExpr != null)
                {
                    var expectedBinExpr = (BinaryExpression) expected;

                    AssertEquals(actualBinExpr.Left, expectedBinExpr.Left);
                    AssertEquals(actualBinExpr.Right, expectedBinExpr.Right);
                    return;
                }

                var actualConstExpr = actual as ConstantExpression;
                if (actualConstExpr != null)
                {
                    var expectedConstExpr = (ConstantExpression) expected;
                    if (actualConstExpr.Type != expectedConstExpr.Type)
                    {
                        Assert.Fail(
                            "Got wrong type for constant expression, expected " + expectedConstExpr.Type +
                            ", but got " + actualConstExpr.Type);
                    }

                    var actualValue = actualConstExpr.Value;
                    var expectedValue = expectedConstExpr.Value;

                    if (!IsEqualOrArrayContentEqual(actualValue, expectedValue, actualConstExpr.Type))
                        Assert.Fail("Constant expression was not of expected value " + expectedValue);
                    return;
                }

                var actualMemberExpr = actual as MemberExpression;
                if (actualMemberExpr != null)
                {
                    var expectedMemberExpr = (MemberExpression) expected;
                    if (actualMemberExpr.Member != expectedMemberExpr.Member)
                        Assert.Fail("Wrong member on memberexpression when comparing expressions..");
                    AssertEquals(actualMemberExpr.Expression, expectedMemberExpr.Expression);
                    return;
                }

                var actualCallExpr = actual as MethodCallExpression;
                if (actualCallExpr != null)
                {
                    var expectedCallExpr = (MethodCallExpression) expected;
                    if (actualCallExpr.Method != expectedCallExpr.Method)
                        Assert.Fail("Wrong method on methodexpression when comparing expressions..");

                    AssertEquals(actualCallExpr.Object, expectedCallExpr.Object);

                    // Recursively check arguments
                    expectedCallExpr
                        .Arguments
                        .Zip(
                            actualCallExpr.Arguments,
                            (ex, ac) =>
                                {
                                    AssertEquals(ac, ex);
                                    return true;
                                }).ToList();
                    return;
                }

                var actualUnaryExpr = actual as UnaryExpression;
                if (actualUnaryExpr != null)
                {
                    var expectedUnaryExpr = (UnaryExpression) expected;
                    Assert.That(
                        actualUnaryExpr.Type, Is.EqualTo(expectedUnaryExpr.Type), "Unary expr was not of expected type.");
                    AssertEquals(actualUnaryExpr.Operand, expectedUnaryExpr.Operand);
                    return;
                }

                var actualParamExpr = actual as ParameterExpression;
                if (actualParamExpr != null)
                {
                    var expectedParamExpr = (ParameterExpression) expected;
                    Assert.That(
                        actualParamExpr.Type, Is.EqualTo(expectedParamExpr.Type), "Parameter was not of expected type.");
                    Assert.That(
                        actualParamExpr.Name,
                        Is.EqualTo(expectedParamExpr.Name),
                        "Parameter did not have expected name.");
                    return;
                }

                var actualNewExpr = actual as NewExpression;
                if (actualNewExpr != null)
                {
                    var expectedNewExpr = (NewExpression) expected;
                    Assert.That(
                        actualNewExpr.Type, Is.EqualTo(expectedNewExpr.Type), "NewExpression was not of expected type.");
                    Assert.That(
                        actualNewExpr.Constructor,
                        Is.EqualTo(expectedNewExpr.Constructor),
                        "NewExpression didn't have expected constructor.");

                    // Recursively check arguments
                    expectedNewExpr
                        .Arguments
                        .Zip(
                            actualNewExpr.Arguments,
                            (ex, ac) =>
                                {
                                    AssertEquals(ac, ex);
                                    return true;
                                }).ToList();

                    return;
                }

                var actualNewArrayExpr = actual as NewArrayExpression;
                if (actualNewArrayExpr != null)
                {
                    var expectedNewArrayExpr = (NewArrayExpression) expected;
                    Assert.That(
                        actualNewArrayExpr.Type,
                        Is.EqualTo(expectedNewArrayExpr.Type),
                        "NewExpression was not of expected type.");

                    // Recursively check arguments
                    expectedNewArrayExpr
                        .Expressions
                        .Zip(
                            actualNewArrayExpr.Expressions,
                            (ex, ac) =>
                                {
                                    AssertEquals(ac, ex);
                                    return true;
                                }).ToList();

                    return;
                }

                throw new NotImplementedException(
                    "Don't know how to compare expression node " + actual + " of nodetype " + actual.NodeType);
            }
            catch
            {
                Console.WriteLine("Expected expression: " + expected + "\r\nActual expression:" + actual);
                throw;
            }
        }

        private static bool IsEqualOrArrayContentEqual(object actual, object expected, Type type)
        {
            if (expected == null || actual == null)
            {
                return actual == null && expected == null;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var actualArray = (Array) actual;
                var expectedArray = (Array) expected;
                if (actualArray.Length != expectedArray.Length)
                    return false;

                return actualArray.Cast<object>()
                                  .Zip(expectedArray.Cast<object>(),
                                       (a, b) => IsEqualOrArrayContentEqual(a, b, elementType))
                                  .All(x => x);
            }

            return actual.Equals(expected);
        }
    }
}