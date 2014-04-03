#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

namespace Pomona.Common.TypeSystem
{
    public struct Maybe<T>
    {
        private static readonly Maybe<T> empty = new Maybe<T>();
        private readonly bool hasValue;
        private readonly T value;


        internal Maybe(T value)
        {
            this.hasValue = true;
            this.value = value;
        }


        public static Maybe<T> Empty
        {
            get { return empty; }
        }

        public bool HasValue
        {
            get { return this.hasValue; }
        }

        public T Value
        {
            get
            {
                if (!this.hasValue)
                    throw new InvalidOperationException("Maybe has no value.");
                return this.value;
            }
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (obj is Maybe<T>)
            {
                var other = (Maybe<T>)obj;
                if (this.hasValue && other.hasValue)
                    return this.value.Equals(other.value);
                return this.hasValue == other.hasValue;
            }

            return false;
        }


        public override int GetHashCode()
        {
            return !this.hasValue ? 0 : this.value.GetHashCode();
        }


        public Maybe<TCast> OfType<TCast>()
        {
            if (this.hasValue && this.value is TCast)
                return new Maybe<TCast>((TCast)((object)this.value));
            return Maybe<TCast>.Empty;
        }


        public T OrDefault(Func<T> defaultFactory = null)
        {
            if (this.hasValue)
                return this.value;
            return defaultFactory == null ? default(T) : defaultFactory();
        }


        public Maybe<TRet> Select<TRet>(Func<T, TRet?> op)
            where TRet : struct
        {
            if (!this.hasValue)
                return Maybe<TRet>.Empty;
            var result = op(this.value);
            return Create(result);
        }


        public Maybe<TRet> Select<TRet>(Func<T, TRet> op)
        {
            if (!this.hasValue)
                return Maybe<TRet>.Empty;
            var result = op(this.value);
            return Create(result);
        }


        public Maybe<TRet> Switch<TRet>(Func<ITypeSwitch, ITypeSwitch<TRet>> cases)
        {
            if (cases == null)
                throw new ArgumentNullException("cases");
            return cases(Switch()).EndSwitch();
        }


        public ITypeSwitch Switch()
        {
            return new TypeSwitch(this);
        }


        public ITypeSwitch<TRet> Switch<TRet>()
        {
            if (this.hasValue)
                return new TypeSwitch<TRet>(this.value);
            return new FinishedTypeSwitch<TRet>(Maybe<TRet>.Empty);
        }


        public Maybe<T> Where(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (this.hasValue && predicate(this.value))
                return this;
            return Empty;
        }


        private static Maybe<TRet> Create<TRet>(TRet? result) where TRet : struct
        {
            return result.HasValue ? new Maybe<TRet>(result.Value) : Maybe<TRet>.Empty;
        }


        private static Maybe<TRet> Create<TRet>(TRet result)
        {
            return (object)result == null ? Maybe<TRet>.Empty : new Maybe<TRet>(result);
        }

        #region Operators

        public static Maybe<T> operator +(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryPlus(a.Value, b.Value, out result))
                return new Maybe<T>(result);
            return Empty;
        }


        public static Maybe<T> operator /(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryDivide(a.Value, b.Value, out result))
                return new Maybe<T>(result);
            return Empty;
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


        public static Maybe<bool> operator !=(Maybe<T> a, Maybe<T> b)
        {
            return !(a == b);
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


        public static Maybe<T> operator *(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryMultiply(a.Value, b.Value, out result))
                return new Maybe<T>(result);
            return Empty;
        }


        public static Maybe<T> operator -(Maybe<T> a, Maybe<T> b)
        {
            T result;
            if (a.HasValue && b.HasValue && Calculator<T>.TryMinus(a.Value, b.Value, out result))
                return new Maybe<T>(result);
            return Empty;
        }

        #endregion

        #region Nested type: FinishedTypeSwitch

        internal class FinishedTypeSwitch<TRet> : ITypeSwitch<TRet>
        {
            private readonly Maybe<TRet> result;


            public FinishedTypeSwitch(Maybe<TRet> result)
            {
                this.result = result;
            }


            public ICaseThen<TCast, TRet> Case<TCast>()
            {
                return new PassthroughCaseThen<TCast, TRet>(this);
            }


            public ICaseThen<TCast, TRet> Case<TCast>(Func<TCast, bool> predicate)
            {
                return new PassthroughCaseThen<TCast, TRet>(this);
            }


            public ICaseThen<T, TRet> Case(Func<T, bool> predicate)
            {
                return new PassthroughCaseThen<T, TRet>(this);
            }


            public Maybe<TRet> EndSwitch()
            {
                return this.result;
            }
        }

        #endregion

        #region Nested type: ICaseThen

        public interface ICaseThen<TIn>
        {
            ITypeSwitch<TRet> Then<TRet>(Func<TIn, TRet> thenFunc);
        }

        public interface ICaseThen<TIn, TRet>
        {
            ITypeSwitch<TRet> Then(Func<TIn, TRet> thenFunc);
        }

        #endregion

        #region Nested type: ITypeSwitch

        public interface ITypeSwitch
        {
            ICaseThen<TCast> Case<TCast>();
            ICaseThen<TCast> Case<TCast>(Func<TCast, bool> predicate);
            ICaseThen<T> Case(Func<T, bool> predicate);
        }

        public interface ITypeSwitch<TRet>
        {
            ICaseThen<TCast, TRet> Case<TCast>();
            ICaseThen<TCast, TRet> Case<TCast>(Func<TCast, bool> predicate);
            ICaseThen<T, TRet> Case(Func<T, bool> predicate);
            Maybe<TRet> EndSwitch();
        }

        #endregion

        #region Nested type: MatchingCaseThen

        internal class MatchingCaseThen<TIn> : ICaseThen<TIn>
        {
            private readonly Maybe<TIn> value;


            public MatchingCaseThen(Maybe<TIn> value)
            {
                this.value = value;
            }


            public ITypeSwitch<TRet> Then<TRet>(Func<TIn, TRet> thenFunc)
            {
                if (thenFunc == null)
                    throw new ArgumentNullException("thenFunc");
                return new FinishedTypeSwitch<TRet>(this.value.Select(thenFunc));
            }
        }

        internal class MatchingCaseThen<TIn, TRet> : ICaseThen<TIn, TRet>
        {
            private readonly TIn value;


            public MatchingCaseThen(TIn value)
            {
                this.value = value;
            }


            public ITypeSwitch<TRet> Then(Func<TIn, TRet> thenFunc)
            {
                if (thenFunc == null)
                    throw new ArgumentNullException("thenFunc");
                return new FinishedTypeSwitch<TRet>(new Maybe<TRet>(thenFunc(this.value)));
            }
        }

        #endregion

        #region Nested type: NonMatchingCaseThen

        internal class NonMatchingCaseThen<TIn> : ICaseThen<TIn>
        {
            private readonly Maybe<T> value;


            public NonMatchingCaseThen(Maybe<T> value)
            {
                this.value = value;
            }


            public ITypeSwitch<TRet> Then<TRet>(Func<TIn, TRet> thenFunc)
            {
                if (this.value.HasValue)
                    return new TypeSwitch<TRet>(this.value.Value);
                return new FinishedTypeSwitch<TRet>(Maybe<TRet>.Empty);
            }
        }

        #endregion

        #region Nested type: PassthroughCaseThen

        internal class PassthroughCaseThen<TIn, TRet> : ICaseThen<TIn, TRet>
        {
            private readonly ITypeSwitch<TRet> passedSwitch;


            public PassthroughCaseThen(ITypeSwitch<TRet> passedSwitch)
            {
                this.passedSwitch = passedSwitch;
            }


            public ITypeSwitch<TRet> Then(Func<TIn, TRet> thenFunc)
            {
                return this.passedSwitch;
            }
        }

        #endregion

        #region Nested type: TypeSwitch

        internal class TypeSwitch : ITypeSwitch
        {
            private readonly Maybe<T> value;


            public TypeSwitch(Maybe<T> value)
            {
                this.value = value;
            }


            public ICaseThen<TCast> Case<TCast>()
            {
                return Case<TCast>(x => true);
            }


            public ICaseThen<TCast> Case<TCast>(Func<TCast, bool> predicate)
            {
                if (predicate == null)
                    throw new ArgumentNullException("predicate");
                if (!this.value.HasValue)
                    return new MatchingCaseThen<TCast>(Maybe<TCast>.Empty);
                if (this.value.Value is TCast)
                {
                    var castValue = (TCast)((object)this.value.Value);
                    if (predicate(castValue))
                        return new MatchingCaseThen<TCast>(new Maybe<TCast>(castValue));
                }
                return new NonMatchingCaseThen<TCast>(this.value);
            }


            public ICaseThen<T> Case(Func<T, bool> predicate)
            {
                if (predicate == null)
                    throw new ArgumentNullException("predicate");
                if (!this.value.HasValue || predicate(this.value.Value))
                    return new MatchingCaseThen<T>(this.value);
                return new NonMatchingCaseThen<T>(this.value);
            }
        }

        internal class TypeSwitch<TRet> : ITypeSwitch<TRet>
        {
            private readonly T value;


            public TypeSwitch(T value)
            {
                this.value = value;
            }


            public ICaseThen<TCast, TRet> Case<TCast>(Func<TCast, bool> predicate)
            {
                if (predicate == null)
                    throw new ArgumentNullException("predicate");
                if (this.value is TCast)
                {
                    var castValue = (TCast)((object)this.value);
                    if (predicate(castValue))
                        return new MatchingCaseThen<TCast, TRet>(castValue);
                }
                return new PassthroughCaseThen<TCast, TRet>(this);
            }


            public ICaseThen<TCast, TRet> Case<TCast>()
            {
                return Case<TCast>(x => true);
            }


            public ICaseThen<T, TRet> Case(Func<T, bool> predicate)
            {
                if (predicate == null)
                    throw new ArgumentNullException("predicate");
                if (predicate(this.value))
                    return new MatchingCaseThen<T, TRet>(this.value);
                return new PassthroughCaseThen<T, TRet>(this);
            }


            public Maybe<TRet> EndSwitch()
            {
                return Maybe<TRet>.Empty;
            }
        }

        #endregion
    }
}