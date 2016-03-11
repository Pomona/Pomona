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
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Expressions
{
    /// <summary>
    /// A helper type to make constructing expressions easier.
    /// </summary>
    public struct Ex
    {
        private readonly Expression expression;


        private Ex(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            this.expression = expression;
        }


        public Ex Apply(Func<Expression, Expression> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            return func(this.expression);
        }


        public static Ex Const<T>(T constValue)
        {
            return Expression.Constant(constValue, typeof(T));
        }


        public static Ex Const(object constValue, Type constType)
        {
            if (constType != null)
                return Expression.Constant(constValue, constType);
            return Expression.Constant(constValue);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Ex && Equals((Ex)obj);
        }


        public override int GetHashCode()
        {
            return (this.expression != null ? this.expression.GetHashCode() : 0);
        }


        public static LambdaExpression Lambda(Type paramType, Func<Ex, Ex> bodyFunc)
        {
            if (paramType == null)
                throw new ArgumentNullException(nameof(paramType));
            if (bodyFunc == null)
                throw new ArgumentNullException(nameof(bodyFunc));
            var param = Expression.Parameter(paramType);
            var body = bodyFunc(param);
            return Expression.Lambda(body, param);
        }


        public Ex Member(MemberInfo member)
        {
            return Expression.MakeMemberAccess(this.expression, member);
        }


        private bool Equals(Ex other)
        {
            return Equals(this.expression, other.expression);
        }

        #region Operators

        public static Ex operator +(Ex a, Ex b)
        {
            return Expression.Add(a, b);
        }


        public static Ex operator &(Ex a, Ex b)
        {
            return Expression.And(a, b);
        }


        public static Ex operator |(Ex a, Ex b)
        {
            return Expression.Or(a, b);
        }


        public static Ex operator /(Ex a, Ex b)
        {
            return Expression.Divide(a, b);
        }


        public static Ex operator ==(Ex a, Ex b)
        {
            return Expression.Equal(a, b);
        }


        public static Ex operator ^(Ex a, Ex b)
        {
            return Expression.ExclusiveOr(a, b);
        }


        public static Ex operator >(Ex a, Ex b)
        {
            return Expression.GreaterThan(a, b);
        }


        public static Ex operator >=(Ex a, Ex b)
        {
            return Expression.GreaterThanOrEqual(a, b);
        }


        public static implicit operator Ex(Expression expression)
        {
            return new Ex(expression);
        }


        public static implicit operator Expression(Ex ex)
        {
            return ex.expression;
        }


        public static Ex operator !=(Ex a, Ex b)
        {
            return Expression.NotEqual(a, b);
        }


        public static Ex operator <(Ex a, Ex b)
        {
            return Expression.LessThan(a, b);
        }


        public static Ex operator <=(Ex a, Ex b)
        {
            return Expression.LessThanOrEqual(a, b);
        }


        public static Ex operator %(Ex a, Ex b)
        {
            return Expression.Modulo(a, b);
        }


        public static Ex operator *(Ex a, Ex b)
        {
            return Expression.Multiply(a, b);
        }


        public static Ex operator ~(Ex a)
        {
            return Expression.OnesComplement(a);
        }


        public static Ex operator -(Ex a, Ex b)
        {
            return Expression.Subtract(a, b);
        }


        public static Ex operator -(Ex a)
        {
            return Expression.Negate(a);
        }

        #endregion
    }
}