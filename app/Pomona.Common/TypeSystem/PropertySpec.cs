#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    public abstract class PropertySpec : MemberSpec
    {
        private readonly Lazy<PropertySpec> baseDefinition;
        private readonly Lazy<TypeSpec> declaringType;
        private readonly Lazy<PropertyGetter> getter;
        private readonly Lazy<bool> isRequiredForConstructor;
        private readonly Lazy<TypeSpec> propertyType;
        private readonly Lazy<PropertySetter> setter;


        protected PropertySpec(ITypeResolver typeResolver,
                               PropertyInfo propertyInfo,
                               TypeSpec reflectedType)
            : base(typeResolver, propertyInfo)
        {
            if (reflectedType == null)
                throw new ArgumentNullException(nameof(reflectedType));
            ReflectedType = reflectedType;
            this.declaringType = CreateLazy(() => typeResolver.LoadDeclaringType(this));
            this.propertyType = CreateLazy(() => typeResolver.LoadPropertyType(this));
            Flags = typeResolver.LoadPropertyFlags(this);
            this.baseDefinition = CreateLazy(() => typeResolver.LoadBaseDefinition(this));
            this.getter = CreateLazy(() => typeResolver.LoadGetter(this));
            this.setter = CreateLazy(() => typeResolver.LoadSetter(this));
            this.isRequiredForConstructor = CreateLazy(() => ReflectedType.RequiredProperties.Contains(this));
        }


        public virtual PropertySpec BaseDefinition => this.baseDefinition.Value;

        public virtual TypeSpec DeclaringType => this.declaringType.Value;

        public virtual PropertyGetter Getter => this.getter.Value;

        public virtual string JsonName => Name.LowercaseFirstLetter();

        public virtual PropertyInfo PropertyInfo => (PropertyInfo)Member;

        public virtual TypeSpec PropertyType => this.propertyType.Value;

        public virtual TypeSpec ReflectedType { get; }

        public virtual PropertySetter Setter => this.setter.Value;

        private PropertyInfo NormalizedPropertyInfo => PropertyInfo.NormalizeReflectedType();


        public Expression CreateGetterExpression(Expression instance)
        {
            var formula = this.GetPropertyFormula();
            if (formula == null)
                return Expression.MakeMemberAccess(instance, NormalizedPropertyInfo);

            //// TODO: Make some assertions here..
            return FindAndReplaceVisitor.Replace(formula.Body, formula.Parameters[0], instance);
        }


        public virtual object GetValue(object target)
        {
            return GetValue(target, null);
        }


        public virtual object GetValue(object target, IContainer container)
        {
            return Getter.Invoke(target, container);
        }


        public virtual void SetValue(object target, object value)
        {
            SetValue(target, value, null);
        }


        public virtual void SetValue(object target, object value, IContainer container)
        {
            Setter.Invoke(target, value, container);
        }


        public override string ToString()
        {
            return $"{ReflectedType}::{Name}";
        }


        protected internal virtual PropertySpec OnLoadBaseDefinition()
        {
            var propInfoBaseDefinition = PropertyInfo.GetBaseDefinition();

            return
                DeclaringType
                    .BaseType
                    .WalkTree(x => x.BaseType)
                    .SelectMany(x => x.Properties.Where(y => y.PropertyInfo.Equals(propInfoBaseDefinition)))
                    .FirstOrDefault();
        }


        protected internal virtual TypeSpec OnLoadDeclaringType()
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("Unable to load DeclaringType when PropertyInfo is null.");

            var decType = PropertyInfo.DeclaringType;
            return decType != null ? TypeResolver.FromType(decType) : null;
        }


        protected internal virtual PropertyGetter OnLoadGetter()
        {
            if (!PropertyInfo.CanRead)
                return null;
            var param = Expression.Parameter(typeof(object));
            return
                Expression.Lambda<Func<object, IContainer, object>>(
                    Expression.Convert(
                        Expression.Property(Expression.Convert(param, PropertyInfo.DeclaringType), PropertyInfo),
                        typeof(object)),
                    param,
                    Expression.Parameter(typeof(IContainer)));
        }


        protected internal virtual PropertyFlags OnLoadPropertyFlags()
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("Unable to load PropertyFlags when PropertyInfo is null.");

            return (PropertyInfo.CanRead ? PropertyFlags.AllowsFiltering | PropertyFlags.IsReadable : 0) |
                   (PropertyInfo.CanWrite ? PropertyFlags.IsWritable : 0);
        }


        protected internal virtual TypeSpec OnLoadPropertyType()
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("Unable to load PropertyType when PropertyInfo is null.");

            return TypeResolver.FromType(PropertyInfo.PropertyType);
        }


        protected internal virtual PropertySetter OnLoadSetter()
        {
            if (!PropertyInfo.CanWrite)
                return null;

            var selfParam = Expression.Parameter(typeof(object), "x");
            var valueParam = Expression.Parameter(typeof(object), "value");
            var expr = Expression.Lambda<Action<object, object, IContainer>>(
                Expression.Assign(
                    Expression.Property(
                        Expression.Convert(selfParam, PropertyInfo.DeclaringType),
                        PropertyInfo
                        ),
                    Expression.Convert(valueParam, PropertyInfo.PropertyType)
                    ),
                selfParam,
                valueParam,
                Expression.Parameter(typeof(IContainer), "ctx"));

            return new PropertySetter(expr.Compile());
        }

        #region PropertySpec implementation

        public virtual HttpMethod AccessMode => 0;

        public virtual bool IsSerialized => true;

        public virtual HttpMethod ItemAccessMode => 0;

        public PropertyFlags Flags { get; }

        public bool IsReadable => Flags.HasFlag(PropertyFlags.IsReadable);

        public bool IsRequiredForConstructor => this.isRequiredForConstructor.Value;

        public bool IsWritable => Flags.HasFlag(PropertyFlags.IsWritable);

        public string LowerCaseName => Name.ToLowerInvariant();

        public virtual ExpandMode ExpandMode => ExpandMode.Default;

        #endregion
    }
}

