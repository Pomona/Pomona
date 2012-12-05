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
            AssertEquals(actual, (Expression)expected);
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
                    var expectedLambdaExpr = (LambdaExpression)expected;
                    AssertEquals(actualLambdaExpr.Body, expectedLambdaExpr.Body);
                    return;
                }

                var actualBinExpr = actual as BinaryExpression;
                if (actualBinExpr != null)
                {
                    var expectedBinExpr = (BinaryExpression)expected;

                    AssertEquals(actualBinExpr.Left, expectedBinExpr.Left);
                    AssertEquals(actualBinExpr.Right, expectedBinExpr.Right);
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
                        Assert.Fail("Wrong member on memberexpression when comparing expressions..");
                    AssertEquals(actualMemberExpr.Expression, expectedMemberExpr.Expression);
                    return;
                }

                var actualCallExpr = actual as MethodCallExpression;
                if (actualCallExpr != null)
                {
                    var expectedCallExpr = (MethodCallExpression)expected;
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

                var actualParamExpr = actual as ParameterExpression;
                if (actualParamExpr != null)
                {
                    var expectedParamExpr = (ParameterExpression)expected;
                    Assert.That(
                        actualParamExpr.Type, Is.EqualTo(expectedParamExpr.Type), "Parameter was not of expected type.");
                    Assert.That(
                        actualParamExpr.Name,
                        Is.EqualTo(expectedParamExpr.Name),
                        "Parameter did not have expected name.");
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
    }
}