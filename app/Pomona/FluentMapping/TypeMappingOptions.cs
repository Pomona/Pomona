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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Internals;

namespace Pomona.FluentMapping
{
    internal sealed class TypeMappingOptions
    {
        private static readonly MethodInfo getConfiguratorGenericMethod =
            ReflectionHelper.GetMethodDefinition<TypeMappingOptions>(x => x.GetConfigurator<object>());

        private readonly Type declaringType;

        public Type DeclaringType
        {
            get { return declaringType; }
        }

        private readonly IDictionary<string, PropertyMappingOptions> propertyOptions =
            new Dictionary<string, PropertyMappingOptions>();

        private ConstructorSpec constructor;

        private DefaultPropertyInclusionMode defaultPropertyInclusionMode =
            DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;

        private bool? isExposedAsRepository;

        private bool? isIndependentTypeRoot;

        private bool? isUriBaseType;
        private bool? isValueObject;
        private Action<object> onDeserialized;

        private bool? patchAllowed;

        private string pluralName;
        private bool? postAllowed;
        private Type postResponseType;


        public TypeMappingOptions(Type declaringType)
        {
            this.declaringType = declaringType;
        }


        public PropertyInfo ChildToParentProperty { get; set; }

        public ConstructorSpec Constructor
        {
            get { return this.constructor; }
        }

        public DefaultPropertyInclusionMode DefaultPropertyInclusionMode
        {
            get { return this.defaultPropertyInclusionMode; }
            set { this.defaultPropertyInclusionMode = value; }
        }

        public bool? IsExposedAsRepository
        {
            get { return this.isExposedAsRepository; }
        }

        public bool? IsIndependentTypeRoot
        {
            get { return this.isIndependentTypeRoot; }
        }

        public bool? IsUriBaseType
        {
            get { return this.isUriBaseType; }
        }

        public bool? IsValueObject
        {
            get { return this.isValueObject; }
        }

        public Action<object> OnDeserialized
        {
            get { return this.onDeserialized; }
        }

        public PropertyInfo ParentToChildProperty { get; internal set; }

        public bool? PatchAllowed
        {
            get { return this.patchAllowed; }
        }

        public string PluralName
        {
            get { return this.pluralName; }
        }

        public bool? PostAllowed
        {
            get { return this.postAllowed; }
        }

        public Type PostResponseType
        {
            get { return this.postResponseType; }
        }

        public IDictionary<string, PropertyMappingOptions> PropertyOptions
        {
            get { return this.propertyOptions; }
        }


        internal object GetConfigurator(Type exposedAsType)
        {
            if (exposedAsType == null)
                throw new ArgumentNullException("exposedAsType");
            return getConfiguratorGenericMethod.MakeGenericMethod(exposedAsType).Invoke(this, null);
        }


        internal ITypeMappingConfigurator<TDeclaringType> GetConfigurator<TDeclaringType>()
        {
            return new Configurator<TDeclaringType>(this);
        }


        internal PropertyMappingOptions GetPropertyOptions(PropertyInfo propertyInfo)
        {
            var name = propertyInfo.Name;
            var propInfo = this.declaringType.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic
                | (propertyInfo.IsStatic() ? BindingFlags.Static : BindingFlags.Instance));
            if (propInfo == null)
            {
                throw new InvalidOperationException(
                    "No property with name " + name + " found on type " + this.declaringType.FullName);
            }

            return this.propertyOptions.GetOrCreate(propInfo.Name, () => new PropertyMappingOptions(propInfo));
        }


        private PropertyMappingOptions GetPropertyOptions(Expression propertyExpr)
        {
            if (propertyExpr == null)
                throw new ArgumentNullException("propertyExpr");
            var propInfo = propertyExpr.ExtractPropertyInfo();
            var propOptions = this.propertyOptions.GetOrCreate(
                propInfo.Name,
                () => new PropertyMappingOptions(propInfo));
            return propOptions;
        }

        #region Nested type: Configurator

        private class Configurator<TDeclaringType> : TypeMappingConfiguratorBase<TDeclaringType>
        {
            private readonly TypeMappingOptions owner;


            public Configurator(TypeMappingOptions owner)
            {
                this.owner = owner;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreExcludedByDefault()
            {
                this.owner.defaultPropertyInclusionMode = DefaultPropertyInclusionMode.AllPropertiesAreExcludedByDefault;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreIncludedByDefault()
            {
                this.owner.defaultPropertyInclusionMode = DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AllPropertiesRequiresExplicitMapping()
            {
                this.owner.defaultPropertyInclusionMode =
                    DefaultPropertyInclusionMode.AllPropertiesRequiresExplicitMapping;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsChildResourceOf<TParent>(
                Expression<Func<TDeclaringType, TParent>> parentProperty,
                Expression<Func<TParent, IEnumerable<TDeclaringType>>> collectionProperty)
            {
                if (parentProperty == null)
                    throw new ArgumentNullException("parentProperty");
                if (collectionProperty == null)
                    throw new ArgumentNullException("collectionProperty");
                this.owner.ChildToParentProperty = parentProperty.ExtractPropertyInfo();
                this.owner.ParentToChildProperty = collectionProperty.ExtractPropertyInfo();
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> HasChildren<TItem>(Expression<Func<TDeclaringType, IEnumerable<TItem>>> property, Expression<Func<TItem, TDeclaringType>> parentProperty, Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> childConfig, Func<IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>, IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>> options)
            {
                return Include(property, options);
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsEntity()
            {
                throw new NotImplementedException();
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsIndependentTypeRoot()
            {
                if (IsMappingSubclass())
                    return this;
                this.owner.isIndependentTypeRoot = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsUriBaseType()
            {
                this.owner.isUriBaseType = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsValueObject()
            {
                this.owner.isValueObject = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
                Expression<Func<IConstructorControl<TDeclaringType>, TDeclaringType>> constructExpr)
            {
                if (IsMappingSubclass())
                    return this;
                this.owner.constructor = new ConstructorSpec(constructExpr);
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
                Expression<Func<TDeclaringType, IConstructorControl<TDeclaringType>, TDeclaringType>> expr)
            {
                // Constructor fluent definitions should not be inherited to subclasses (because it wouldn't work).
                if (IsMappingSubclass())
                    return this;

                throw new NotImplementedException();
            }


            public override ITypeMappingConfigurator<TDeclaringType> Exclude(Expression<Func<TDeclaringType, object>> property)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                var propOptions = this.owner.GetPropertyOptions(property);
                propOptions.InclusionMode = PropertyInclusionMode.Excluded;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> ExposedAsRepository()
            {
                this.owner.isExposedAsRepository = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
                Expression<Func<TDeclaringType, TPropertyType>> property,
                Func
                    <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                        IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                var propOptions = this.owner.GetPropertyOptions(property);

                propOptions.InclusionMode = PropertyInclusionMode.Included;

                if (options != null)
                    options(new PropertyOptionsBuilder<TDeclaringType, TPropertyType>(propOptions));

                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> OnDeserialized(Action<TDeclaringType> action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                this.owner.onDeserialized = x => action((TDeclaringType)x);
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PatchAllowed()
            {
                this.owner.patchAllowed = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PatchDenied()
            {
                this.owner.patchAllowed = false;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostAllowed()
            {
                this.owner.postAllowed = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostDenied()
            {
                this.owner.postAllowed = false;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostReturns<TPostResponseType>()
            {
                return PostReturns(typeof(TPostResponseType));
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostReturns(Type type)
            {
                this.owner.postResponseType = type;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> WithPluralName(string pluralName)
            {
                this.owner.pluralName = pluralName;
                return this;
            }


            private bool IsMappingSubclass()
            {
                return !this.owner.declaringType.IsAssignableFrom(typeof(TDeclaringType));
            }
        }

        #endregion
    }
}