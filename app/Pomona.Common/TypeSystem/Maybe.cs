#region License

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
    }
}