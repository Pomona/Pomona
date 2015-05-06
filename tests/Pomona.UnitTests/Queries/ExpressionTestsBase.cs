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
using System.Linq.Expressions;

using Pomona.Common.Internals;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Queries
{
    public abstract class ExpressionTestsBase
    {
        protected static void AssertExpressionEquals<T, TReturn>(
            Expression<Func<T, TReturn>> actual,
            Expression<Func<T, TReturn>> expected)
        {
            AssertExpressionEquals(actual, (Expression)expected);
        }


        protected static void AssertExpressionEquals(Expression actual, Expression expected)
        {
            actual.Visit<NormalizeExpressionToRoslynStyle>().AssertEquals(expected.Visit<NormalizeExpressionToRoslynStyle>());
        }


        public class NormalizeExpressionToRoslynStyle : ExpressionVisitor
        {
            private static readonly Type[] enumUnderlyingTypes = { typeof(byte), typeof(int), typeof(long) };


            protected override Expression VisitBinary(BinaryExpression node)
            {
                if ((node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual))
                {
                    var left = Visit(node.Left);
                    var right = Visit(node.Right);

                    // Given the expression
                    //     (string x) => x == null
                    // Roslyn will generate
                    //     Expression.Equals(
                    // Old compiler will generate
                    //     (string x) => x == Expression.Constant<object>(null);
                    if (node.Method == null)
                        NormalizeReferenceComparisonWithNull(ref left, ref right);

                    // Given the expression
                    //     (Enum x) => x == Enum.Member
                    // Roslyn will generate:
                    //     (Enum x) => (int)x == (int)Enum.Member
                    // Old compiler will generate:
                    //     (Enum x) => (int)x = 1234   (when Enum.Member value is 1234)
                    NormalizeNonNullableEnumComparisonWithConstant(ref left, ref right);

                    return Expression.MakeBinary(node.NodeType, left, right);
                }
                return base.VisitBinary(node);
            }


            protected override Expression VisitUnary(UnaryExpression node)
            {
                var nodeUnderlyingType = Nullable.GetUnderlyingType(node.Type);
                var operandAsConstant = node.Operand as ConstantExpression;
                if (operandAsConstant != null && nodeUnderlyingType != null && operandAsConstant.Type.IsEnum
                    && nodeUnderlyingType == operandAsConstant.Type.GetEnumUnderlyingType()
                    && operandAsConstant.Value != null)
                {
                    // Given the expression
                    //     (Enum? x) => x == Enum.Member => (Enum? x)
                    // Roslyn will generate:
                    //     (Enum? x) => (int?)x == (int?)((Enum?)Enum.Member) <--- notice the double cast from Enum to Enum? to int?
                    // The old compiler will generate:
                    //     (Enum? x) => (int?)x == (int?)EnumMember <--- just a single cast from Enum to int?
                    //
                    // We'll normalize to the way Roslyn does this:
                    return
                        Expression.Convert(
                            Expression.Convert(operandAsConstant,
                                               typeof(Nullable<>).MakeGenericType(operandAsConstant.Type)), node.Type);
                }
                return base.VisitUnary(node);
            }


            private void NormalizeNonNullableEnumComparisonWithConstant(ref Expression left,
                                                                        ref Expression right,
                                                                        bool tryAgainSwapped = true)
            {
                var unaryLeft = left as UnaryExpression;
                var underlyingType = left.Type;
                Type enumType;
                if (enumUnderlyingTypes.Contains(underlyingType) &&
                    unaryLeft != null &&
                    left.NodeType == ExpressionType.Convert &&
                    (enumType = unaryLeft.Operand.Type).IsEnum)
                {
                    if (right.Type == underlyingType && right.NodeType == ExpressionType.Constant)
                    {
                        var rightConstant = (ConstantExpression)right;
                        right = Expression.Convert(Expression.Constant(Enum.ToObject(unaryLeft.Operand.Type, rightConstant.Value),
                                                                       enumType), underlyingType);
                        return;
                    }
                }

                if (tryAgainSwapped)
                    NormalizeNonNullableEnumComparisonWithConstant(ref right, ref left, false);
            }


            private void NormalizeReferenceComparisonWithNull(ref Expression left, ref Expression right, bool tryAgainSwapped = true)
            {
                var rightAsConstant = right as ConstantExpression;
                if (!left.Type.IsValueType && left.Type != right.Type && right.Type == typeof(object) && rightAsConstant != null
                    && rightAsConstant.Value == null)
                {
                    right = Expression.Constant(null, left.Type);
                    return;
                }
                if (tryAgainSwapped)
                    NormalizeReferenceComparisonWithNull(ref right, ref left, false);
            }
        }
    }
}