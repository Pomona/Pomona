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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.Proxies
{
    public class PropertyWrapper<TOwner, TPropType>
    {
        private readonly PropertyInfo propertyInfo;
        private Func<TOwner, TPropType> getter;
        private Expression<Func<TOwner, TPropType>> getterExpression;
        private Action<TOwner, TPropType> setter;
        private Expression<Action<TOwner, TPropType>> setterExpression;


        public PropertyWrapper(string propertyName)
        {
            var ownerType = typeof(TOwner);
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            this.propertyInfo = TypeUtils.AllBaseTypesAndInterfaces(ownerType)
                .Select(t => t.GetProperty(propertyName, bindingFlags))
                .FirstOrDefault(t => t != null);

            if (this.propertyInfo == null)
                throw new MissingMemberException(String.Format("Could not wrap property {0}.", propertyName));
        }


        public Func<TOwner, TPropType> Getter
        {
            get { return this.getter ?? (this.getter = GetterExpression.Compile()); }
        }

        public Expression<Func<TOwner, TPropType>> GetterExpression
        {
            get
            {
                if (this.getterExpression == null)
                {
                    var ownerType = typeof(TOwner);

                    var getterSelfParam = Expression.Parameter(ownerType, "x");
                    this.getterExpression =
                        Expression.Lambda<Func<TOwner, TPropType>>(
                            Expression.MakeMemberAccess(getterSelfParam, this.propertyInfo), getterSelfParam);
                }

                return this.getterExpression;
            }
        }

        public string Name
        {
            get { return this.propertyInfo.Name; }
        }

        public PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }

        public Action<TOwner, TPropType> Setter
        {
            get { return this.setter ?? (this.setter = SetterExpression.Compile()); }
        }

        public Expression<Action<TOwner, TPropType>> SetterExpression
        {
            get
            {
                if (this.setterExpression == null)
                {
                    var ownerType = typeof(TOwner);

                    var setterSelfParam = Expression.Parameter(ownerType, "x");
                    var setterValueParam = Expression.Parameter(typeof(TPropType), "value");

                    this.setterExpression =
                        Expression.Lambda<Action<TOwner, TPropType>>(
                            Expression.Assign(Expression.Property(setterSelfParam, this.propertyInfo), setterValueParam),
                            setterSelfParam,
                            setterValueParam);
                }
                return this.setterExpression;
            }
        }


        public override string ToString()
        {
            return String.Format("{0}.{1}", this.propertyInfo.DeclaringType, Name);
        }


        public TPropType Get(TOwner obj)
        {
            return Getter(obj);
        }


        public void Set(TOwner obj, TPropType value)
        {
            Setter(obj, value);
        }
    }
}