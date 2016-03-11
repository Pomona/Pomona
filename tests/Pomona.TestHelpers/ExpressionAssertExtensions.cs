#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.ObjectModel;
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

                var actualCondExpr = actual as ConditionalExpression;
                if (actualCondExpr != null)
                {
                    var expectedCondExpr = (ConditionalExpression)expected;
                    AssertEquals(actualCondExpr.Test, expectedCondExpr.Test);
                    AssertEquals(actualCondExpr.IfTrue, expectedCondExpr.IfTrue);
                    AssertEquals(actualCondExpr.IfFalse, expectedCondExpr.IfFalse);
                    return;
                }

                var actualTypeBinExpr = actual as TypeBinaryExpression;
                if (actualTypeBinExpr != null)
                {
                    var expectedTypeBinExpr = (TypeBinaryExpression)expected;

                    AssertEquals(actualTypeBinExpr.Expression, expectedTypeBinExpr.Expression);
                    if (actualTypeBinExpr.TypeOperand != expectedTypeBinExpr.TypeOperand)
                    {
                        Assert.Fail("Expected TypeOperand " + expectedTypeBinExpr.TypeOperand + " got nodetype " +
                                    actualTypeBinExpr.TypeOperand);
                    }

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

                    var actualValue = actualConstExpr.Value;
                    var expectedValue = expectedConstExpr.Value;

                    if (!IsEqualOrArrayContentEqual(actualValue, expectedValue, actualConstExpr.Type))
                        Assert.Fail("Constant expression was not of expected value " + expectedValue);
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
                    AssertEquals(actualCallExpr.Arguments, expectedCallExpr.Arguments);
                    return;
                }

                var actualUnaryExpr = actual as UnaryExpression;
                if (actualUnaryExpr != null)
                {
                    var expectedUnaryExpr = (UnaryExpression)expected;
                    Assert.That(
                        actualUnaryExpr.Type, Is.EqualTo(expectedUnaryExpr.Type), "Unary expr was not of expected type.");
                    AssertEquals(actualUnaryExpr.Operand, expectedUnaryExpr.Operand);
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

                var actualMemberInit = actual as MemberInitExpression;
                if (actualMemberInit != null)
                {
                    var expectedMemberInit = (MemberInitExpression)expected;
                    AssertEquals(actualMemberInit.NewExpression, expectedMemberInit.NewExpression);
                    Assert.That(actualMemberInit.Bindings.Count, Is.EqualTo(expectedMemberInit.Bindings.Count),
                                "MemberInitExpression has different number of Bindings");
                    AssertEquals(actualMemberInit.Bindings, expectedMemberInit.Bindings);
                    return;
                }

                var actualNewExpr = actual as NewExpression;
                if (actualNewExpr != null)
                {
                    var expectedNewExpr = (NewExpression)expected;
                    Assert.That(
                        actualNewExpr.Type, Is.EqualTo(expectedNewExpr.Type), "NewExpression was not of expected type.");
                    Assert.That(
                        actualNewExpr.Constructor,
                        Is.EqualTo(expectedNewExpr.Constructor),
                        "NewExpression didn't have expected constructor.");

                    // Recursively check arguments
                    AssertEquals(actualNewExpr.Arguments, expectedNewExpr.Arguments);

                    return;
                }

                var actualNewArrayExpr = actual as NewArrayExpression;
                if (actualNewArrayExpr != null)
                {
                    var expectedNewArrayExpr = (NewArrayExpression)expected;
                    Assert.That(
                        actualNewArrayExpr.Type,
                        Is.EqualTo(expectedNewArrayExpr.Type),
                        "NewExpression was not of expected type.");

                    // Recursively check arguments
                    AssertEquals(actualNewArrayExpr.Expressions, expectedNewArrayExpr.Expressions);

                    return;
                }

                var actualListInit = actual as ListInitExpression;
                if (actualListInit != null)
                {
                    var expectedListInit = (ListInitExpression)expected;
                    AssertEquals(actualListInit.NewExpression, expectedListInit.NewExpression);
                    AssertEquals(actualListInit.Initializers, expectedListInit.Initializers);
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


        private static void AssertEquals(MemberBinding actual, MemberBinding expected)
        {
            Assert.That(actual.BindingType, Is.EqualTo(expected.BindingType));
            Assert.That(actual.Member, Is.EqualTo(expected.Member));

            switch (actual.BindingType)
            {
                case MemberBindingType.Assignment:
                    var acAssignment = (MemberAssignment)actual;
                    var exAssignment = (MemberAssignment)expected;
                    AssertEquals(acAssignment.Expression, exAssignment.Expression);
                    break;

                case MemberBindingType.ListBinding:
                    var acListBinding = (MemberListBinding)actual;
                    var exListBinding = (MemberListBinding)expected;
                    AssertEquals(acListBinding.Initializers, exListBinding.Initializers);
                    break;

                case MemberBindingType.MemberBinding:
                    var acMemberBinding = (MemberMemberBinding)actual;
                    var exMemberBinding = (MemberMemberBinding)expected;
                    AssertEquals(acMemberBinding.Bindings, exMemberBinding.Bindings);
                    break;
            }
        }


        private static void AssertEquals(ReadOnlyCollection<MemberBinding> actual, ReadOnlyCollection<MemberBinding> expected)
        {
            AssertEquals(actual, expected, AssertEquals);
        }


        private static void AssertEquals<T>(ReadOnlyCollection<T> actual, ReadOnlyCollection<T> expected, Action<T, T> itemAssertion)
        {
            if (actual == null)
                throw new ArgumentNullException(nameof(actual));
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));
            if (itemAssertion == null)
                throw new ArgumentNullException(nameof(itemAssertion));
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (int i = 0; i < actual.Count; i++)
                itemAssertion(actual[i], expected[i]);
        }


        private static void AssertEquals(ReadOnlyCollection<ElementInit> actual, ReadOnlyCollection<ElementInit> expected)
        {
            AssertEquals(actual, expected, AssertEquals);
        }


        private static void AssertEquals(ElementInit actual, ElementInit expected)
        {
            Assert.That(actual.AddMethod, Is.EqualTo(expected.AddMethod));
            AssertEquals(actual.Arguments, expected.Arguments);
        }


        private static void AssertEquals(ReadOnlyCollection<Expression> actual, ReadOnlyCollection<Expression> expected)
        {
            AssertEquals(actual, expected, AssertEquals);
        }


        private static bool IsEqualOrArrayContentEqual(object actual, object expected, Type type)
        {
            if (expected == null || actual == null)
                return actual == null && expected == null;

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var actualArray = (Array)actual;
                var expectedArray = (Array)expected;
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