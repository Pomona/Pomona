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

namespace Pomona.Common.TypeSystem
{
    public abstract class PropertySpec : MemberSpec
    {
        private readonly Lazy<PropertySpec> baseDefinition;
        private readonly Lazy<TypeSpec> declaringType;
        private readonly Lazy<Func<object, IContainer, object>> getter;
        private readonly Lazy<bool> isRequiredForConstructor;
        private readonly PropertyFlags propertyFlags;
        private readonly Lazy<TypeSpec> propertyType;
        private readonly Lazy<TypeSpec> reflectedType;
        private readonly Lazy<Action<object, object, IContainer>> setter;


        protected PropertySpec(ITypeResolver typeResolver,
            PropertyInfo propertyInfo,
            Func<TypeSpec> reflectedType = null)
            : base(typeResolver, propertyInfo)
        {
            this.reflectedType = CreateLazy(reflectedType ?? (() => typeResolver.LoadReflectedType(this)));
            this.declaringType = CreateLazy(() => typeResolver.LoadDeclaringType(this));
            this.propertyType = CreateLazy(() => typeResolver.LoadPropertyType(this));
            this.propertyFlags = typeResolver.LoadPropertyFlags(this);
            this.baseDefinition = CreateLazy(() => typeResolver.LoadBaseDefinition(this));
            this.getter = CreateLazy(() => typeResolver.LoadGetter(this));
            this.setter = CreateLazy(() => typeResolver.LoadSetter(this));
            this.isRequiredForConstructor = CreateLazy(() => ReflectedType.RequiredProperties.Contains(this));
        }


        public virtual PropertySpec BaseDefinition
        {
            get { return this.baseDefinition.Value; }
        }

        public virtual TypeSpec DeclaringType
        {
            get { return this.declaringType.Value; }
        }

        public virtual Func<object, IContainer, object> GetterFunc
        {
            get { return this.getter.Value; }
        }

        public virtual string JsonName
        {
            get { return Name.LowercaseFirstLetter(); }
        }

        public virtual PropertyInfo PropertyInfo
        {
            get { return (PropertyInfo)Member; }
        }

        public virtual TypeSpec PropertyType
        {
            get { return this.propertyType.Value; }
        }

        public virtual TypeSpec ReflectedType
        {
            get { return this.reflectedType.Value; }
        }

        public virtual Action<object, object, IContainer> SetterDelegate
        {
            get { return this.setter.Value; }
        }

        private PropertyInfo NormalizedPropertyInfo
        {
            get { return PropertyInfo.NormalizeReflectedType(); }
        }

        #region PropertySpec implementation

        public virtual HttpMethod AccessMode
        {
            get { return 0; }
        }

        public virtual bool IsSerialized
        {
            get { return true; }
        }

        public virtual HttpMethod ItemAccessMode
        {
            get
            {
                // Only for collections..
                return 0;
            }
        }

        public PropertyFlags Flags
        {
            get { return this.propertyFlags; }
        }

        public bool IsReadable
        {
            get { return this.propertyFlags.HasFlag(PropertyFlags.IsReadable); }
        }

        public bool IsRequiredForConstructor
        {
            get { return this.isRequiredForConstructor.Value; }
        }

        public bool IsWritable
        {
            get { return this.propertyFlags.HasFlag(PropertyFlags.IsWritable); }
        }

        public string LowerCaseName
        {
            get { return Name.ToLowerInvariant(); }
        }

        #endregion

        public virtual object GetValue(object target)
        {
            return GetValue(target, null);
        }


        public virtual object GetValue(object target, IContainer container)
        {
            container = container ?? new NoContainer();
            return GetterFunc(target, container);
        }


        public virtual void SetValue(object target, object value)
        {
            SetValue(target, value, null);
        }


        public virtual void SetValue(object target, object value, IContainer container)
        {
            container = container ?? new NoContainer();
            SetterDelegate(target, value, container);
        }


        public override string ToString()
        {
            return string.Format("{0}::{1}", ReflectedType, Name);
        }


        public Expression CreateGetterExpression(Expression instance)
        {
            var formula = this.GetPropertyFormula();
            if (formula == null)
                return Expression.MakeMemberAccess(instance, NormalizedPropertyInfo);

            //// TODO: Make some assertions here..
            return FindAndReplaceVisitor.Replace(formula.Body, formula.Parameters[0], instance);
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


        protected internal virtual Func<object, IContainer, object> OnLoadGetter()
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
                    Expression.Parameter(typeof(IContainer))).Compile();
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


        protected internal virtual TypeSpec OnLoadReflectedType()
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("Unable to load ReflectedType when PropertyInfo is null.");

            return TypeResolver.FromType(PropertyInfo.ReflectedType);
        }


        protected internal virtual Action<object, object, IContainer> OnLoadSetter()
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

            return expr.Compile();
        }
    }
}