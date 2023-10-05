#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

