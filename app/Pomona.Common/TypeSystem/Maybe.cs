using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;


namespace Pomona.Common.TypeSystem
{
    public static class TypeSwitchExtensions
    {
        private class MatchedTypeSwitchContext<TRet> : ITypeSwitchContext<TRet>
        {
            private readonly TRet result;

            internal TRet Result
            {
                get { return this.result; }
            }


            internal MatchedTypeSwitchContext(TRet result)
            {
                this.result = result;
            }


            public ITypeSwitchContext<TRet> Case<TCast>(Func<TCast, TRet> func)
            {
                return this;
            }
        }

        private class SearchingTypeSwitchContext<TRet> : ITypeSwitchContext<TRet>
        {
            private readonly object value;


            internal SearchingTypeSwitchContext(object value)
            {
                this.value = value;
            }


            public ITypeSwitchContext<TRet> Case<TCast>(Func<TCast, TRet> func)
            {
                if (this.value is TCast)
                    return new MatchedTypeSwitchContext<TRet>(func((TCast)this.value));
                return this;
            }
        }


        public static TRet SwitchType<TRet>(this object source, Func<ITypeSwitchContext<TRet>, ITypeSwitchContext<TRet>> cases, Func<TRet> defaultFactory = null)
        {
            var match = cases(new SearchingTypeSwitchContext<TRet>(source)) as MatchedTypeSwitchContext<TRet>;
            return match != null ? match.Result : (defaultFactory != null ? defaultFactory() : default(TRet));
        }

    }


    public interface ITypeSwitchContext<TRet>
    {
        ITypeSwitchContext<TRet> Case<TCast>(Func<TCast, TRet> func);
    }

    public struct Maybe<T>
    {
        private readonly bool hasValue;
        private readonly T value;
        private static readonly Maybe<T> empty = new Maybe<T>();

        public static Maybe<T> Empty {get { return empty; }}

        internal Maybe(T value)
        {
            this.hasValue = true;
            this.value = value;
        }


        public T Value
        {
            get
            {
                if (!hasValue)
                    throw new InvalidOperationException("Maybe has no value.");
                return value;
            }
        }

        public T OrDefault(Func<T> defaultFactory = null)
        {
            if (hasValue)
                return value;
            return defaultFactory == null ? default(T) : defaultFactory();
        }

        public Maybe<TRet> Select<TRet>(Func<T, TRet?> op)
            where TRet : struct
        {
            if (!hasValue)
                return Maybe<TRet>.Empty;
            var result = op(value);
            return Create(result);
        }

        private static Maybe<TRet> Create<TRet>(TRet? result) where TRet : struct
        {
            return result.HasValue ? new Maybe<TRet>(result.Value) : Maybe<TRet>.Empty;
        }


        public Maybe<T> Where(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");
            if (hasValue && predicate(value))
                return this;
            return Empty;
        }

        public bool HasValue
        {
            get { return this.hasValue; }
        }


        public Maybe<TCast> OfType<TCast>()
        {
            if (hasValue && value is TCast)
                return new Maybe<TCast>((TCast)((object)value));
            return Maybe<TCast>.Empty;
        }

        public Maybe<TRet> Select<TRet>(Func<T, TRet> op)
        {
            if (!hasValue)
                return Maybe<TRet>.Empty;
            var result = op(value);
            return Create(result);
        }


        private static Maybe<TRet> Create<TRet>(TRet result)
        {
            return (object)result == null ? Maybe<TRet>.Empty : new Maybe<TRet>(result);
        }


        public override int GetHashCode()
        {
            return !this.hasValue ? 0 : this.value.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (obj is Maybe<T>)
            {
                var other = (Maybe<T>)obj;
                if (hasValue && other.hasValue)
                    return value.Equals(other.value);
                return hasValue == other.hasValue;
            }

            return false;
        }

        public static Maybe<bool> operator ==(Maybe<T> a, Maybe<T> b)
        {
            if (a.hasValue && b.hasValue)
            {
                bool result;
                if (Calculator<T>.TryEquals(a.Value, b.Value, out result))
                    return new Maybe<bool>(result);
            }
            return Maybe<bool>.Empty;
        }

        public static explicit operator T(Maybe<T> maybe)
        {
            return maybe.Value;
        }

        public static Maybe<T> operator !(Maybe<T> a)
        {
            if (!a.HasValue)
                return a;

            T result;
            if (Calculator<T>.TryNegate(a.Value, out result))
                return new Maybe<T>(result);
            return Empty;
        }

        public static Maybe<bool> operator !=(Maybe<T> a, Maybe<T> b)
        {
            return !(a == b);
        }


        public static Maybe<T> operator +(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryPlus(a.Value, b.Value, out result))
            {
                return new Maybe<T>(result);
            }
            return Empty;
        }


        public static Maybe<T> operator -(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryMinus(a.Value, b.Value, out result))
            {
                return new Maybe<T>(result);
            }
            return Empty;
        }


        public static Maybe<T> operator *(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryMultiply(a.Value, b.Value, out result))
            {
                return new Maybe<T>(result);
            }
            return Empty;
        }


        public static Maybe<T> operator /(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryDivide(a.Value, b.Value, out result))
            {
                return new Maybe<T>(result);
            }
            return Empty;
        }
    }

    public static class Calculator<T>
    {
        private readonly static System.Lazy<Func<T, T, T>> addFunc;
        private readonly static System.Lazy<Func<T, T, T>> minusFunc;
        private readonly static System.Lazy<Func<T, T, T>> multiplyFunc;
        private readonly static System.Lazy<Func<T, T, T>> divideFunc;
        private readonly static System.Lazy<Func<T, T, bool>> equalFunc;
        private readonly static System.Lazy<Func<T, T>> negateFunc;

        static Calculator()
        {
            addFunc = GetBinOpMethod<T>(ExpressionType.Add);
            minusFunc = GetBinOpMethod<T>(ExpressionType.Subtract);
            multiplyFunc = GetBinOpMethod<T>(ExpressionType.Multiply);
            divideFunc = GetBinOpMethod<T>(ExpressionType.Divide);
            equalFunc = GetBinOpMethod<bool>(ExpressionType.Equal);
            negateFunc = GetUnaryOpMethod<T>(ExpressionType.Negate);
        }


        private static System.Lazy<Func<T, T, TRet>> GetBinOpMethod<TRet>(ExpressionType binaryType)
        {
            return new System.Lazy<Func<T, T, TRet>>(() =>
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

        private static System.Lazy<Func<T, TRet>> GetUnaryOpMethod<TRet>(ExpressionType unaryType)
        {
            return new System.Lazy<Func<T, TRet>>(() =>
            {
                try
                {
                    var a = Expression.Parameter(typeof(T));
                    return Expression.Lambda<Func<T, TRet>>(Expression.MakeUnary(unaryType, a, typeof(TRet)), a).Compile();
                }
                catch (Exception)
                {
                    return null;
                }
            });
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


        public static bool TryPlus(T a, T b, out T result)
        {
            return TryInvokeOperator(a, b, out result, addFunc);
        }


        private static bool TryInvokeOperator<TRet>(T a, T b, out TRet result, System.Lazy<Func<T, T, TRet>> operatorMethod)
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

        private static bool TryInvokeUnaryOperator<TRet>(T a, out TRet result, System.Lazy<Func<T, TRet>> operatorMethod)
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



        public static T Plus(T a, T b)
        {
            return InvokeOperator(a, b, addFunc);
        }


        public static T Minus(T a, T b)
        {
            return InvokeOperator(a, b, minusFunc);
        }


        public static T Multiply(T a, T b)
        {
            return InvokeOperator(a, b, multiplyFunc);
        }
        public static bool TryDivide(T a, T b, out T result)
        {
            return TryInvokeOperator(a, b, out result, divideFunc);
        }

        public static T Negate(T a)
        {
            return InvokeUnaryOperator(a, negateFunc);
        }

        public static T Divide(T a, T b)
        {
            return InvokeOperator(a, b, divideFunc);
        }

        private static T InvokeOperator(T a, T b, System.Lazy<Func<T, T, T>> operatorMethod)
        {
            var func = operatorMethod.Value;
            if (func == null)
                throw new InvalidOperationException("Add not defined for type.");
            return func(a, b);
        }

        private static T InvokeUnaryOperator(T a, System.Lazy<Func<T, T>> operatorMethod)
        {
            var func = operatorMethod.Value;
            if (func == null)
                throw new InvalidOperationException("Add not defined for type.");
            return func(a);
        }

        public static bool TryNegate(T a, out T result)
        {
            return TryInvokeUnaryOperator(a, out result, negateFunc);
        }
    }
}