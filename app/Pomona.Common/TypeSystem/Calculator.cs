#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    public static class Calculator<T>
    {
        private static readonly Lazy<Func<T, T, T>> addFunc;
        private static readonly Lazy<Func<T, T, T>> divideFunc;
        private static readonly Lazy<Func<T, T, bool>> equalFunc;
        private static readonly Lazy<Func<T, T, T>> minusFunc;
        private static readonly Lazy<Func<T, T, T>> multiplyFunc;
        private static readonly Lazy<Func<T, T>> negateFunc;


        static Calculator()
        {
            addFunc = GetBinOpMethod<T>(ExpressionType.Add);
            minusFunc = GetBinOpMethod<T>(ExpressionType.Subtract);
            multiplyFunc = GetBinOpMethod<T>(ExpressionType.Multiply);
            divideFunc = GetBinOpMethod<T>(ExpressionType.Divide);
            equalFunc = GetBinOpMethod<bool>(ExpressionType.Equal);
            negateFunc = GetUnaryOpMethod<T>(ExpressionType.Negate);
        }


        public static T Divide(T a, T b)
        {
            return InvokeOperator(a, b, divideFunc);
        }


        public static T Minus(T a, T b)
        {
            return InvokeOperator(a, b, minusFunc);
        }


        public static T Multiply(T a, T b)
        {
            return InvokeOperator(a, b, multiplyFunc);
        }


        public static T Negate(T a)
        {
            return InvokeUnaryOperator(a, negateFunc);
        }


        public static T Plus(T a, T b)
        {
            return InvokeOperator(a, b, addFunc);
        }


        public static bool TryDivide(T a, T b, out T result)
        {
            return TryInvokeOperator(a, b, out result, divideFunc);
        }


        public static bool TryEquals(T a, T b, out bool result)
        {
            return TryInvokeOperator(a, b, out result, equalFunc);
        }


        public static bool TryMinus(T a, T b, out T result)
        {
            return TryInvokeOperator(a, b, out result, minusFunc);
        }


        public static bool TryMultiply(T a, T b, out T result)
        {
            return TryInvokeOperator(a, b, out result, multiplyFunc);
        }


        public static bool TryNegate(T a, out T result)
        {
            return TryInvokeUnaryOperator(a, out result, negateFunc);
        }


        public static bool TryPlus(T a, T b, out T result)
        {
            return TryInvokeOperator(a, b, out result, addFunc);
        }


        private static Lazy<Func<T, T, TRet>> GetBinOpMethod<TRet>(ExpressionType binaryType)
        {
            return new Lazy<Func<T, T, TRet>>(() =>
            {
                try
                {
                    var a = Expression.Parameter(typeof(T));
                    var b = Expression.Parameter(typeof(T));
                    return Expression.Lambda<Func<T, T, TRet>>(Expression.MakeBinary(binaryType, a, b), a, b).Compile();
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }


        private static Lazy<Func<T, TRet>> GetUnaryOpMethod<TRet>(ExpressionType unaryType)
        {
            return new Lazy<Func<T, TRet>>(() =>
            {
                try
                {
                    var a = Expression.Parameter(typeof(T));
                    return
                        Expression.Lambda<Func<T, TRet>>(Expression.MakeUnary(unaryType, a, typeof(TRet)), a).Compile();
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }


        private static T InvokeOperator(T a, T b, Lazy<Func<T, T, T>> operatorMethod)
        {
            var func = operatorMethod.Value;
            if (func == null)
                throw new InvalidOperationException("Add not defined for type.");
            return func(a, b);
        }


        private static T InvokeUnaryOperator(T a, Lazy<Func<T, T>> operatorMethod)
        {
            var func = operatorMethod.Value;
            if (func == null)
                throw new InvalidOperationException("Add not defined for type.");
            return func(a);
        }


        private static bool TryInvokeOperator<TRet>(T a,
                                                    T b,
                                                    out TRet result,
                                                    Lazy<Func<T, T, TRet>> operatorMethod)
        {
            var func = operatorMethod.Value;
            if (func == null)
            {
                result = default(TRet);
                return false;
            }
            result = func(a, b);
            return true;
        }


        private static bool TryInvokeUnaryOperator<TRet>(T a, out TRet result, Lazy<Func<T, TRet>> operatorMethod)
        {
            var func = operatorMethod.Value;
            if (func == null)
            {
                result = default(TRet);
                return false;
            }
            result = func(a);
            return true;
        }
    }
}

