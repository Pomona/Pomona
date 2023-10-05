#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
        private Func<TOwner, TPropType> getter;
        private Expression<Func<TOwner, TPropType>> getterExpression;
        private Action<TOwner, TPropType> setter;
        private Expression<Action<TOwner, TPropType>> setterExpression;


        public PropertyWrapper(string propertyName)
        {
            var ownerType = typeof(TOwner);
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo = TypeUtils.AllBaseTypesAndInterfaces(ownerType)
                                    .Select(t => t.GetProperty(propertyName, bindingFlags))
                                    .FirstOrDefault(t => t != null);

            if (PropertyInfo == null)
                throw new MissingMemberException($"Could not wrap property {propertyName}.");
        }


        public Func<TOwner, TPropType> Getter => this.getter ?? (this.getter = GetterExpression.Compile());

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
                            Expression.MakeMemberAccess(getterSelfParam, PropertyInfo), getterSelfParam);
                }

                return this.getterExpression;
            }
        }

        public string Name => PropertyInfo.Name;

        public PropertyInfo PropertyInfo { get; }

        public Action<TOwner, TPropType> Setter => this.setter ?? (this.setter = SetterExpression.Compile());

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
                            Expression.Assign(Expression.Property(setterSelfParam, PropertyInfo), setterValueParam),
                            setterSelfParam,
                            setterValueParam);
                }
                return this.setterExpression;
            }
        }


        public TPropType Get(TOwner obj)
        {
            return Getter(obj);
        }


        public void Set(TOwner obj, TPropType value)
        {
            Setter(obj, value);
        }


        public override string ToString()
        {
            return $"{PropertyInfo.DeclaringType}.{Name}";
        }
    }
}
