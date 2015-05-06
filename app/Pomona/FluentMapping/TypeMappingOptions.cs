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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal sealed class TypeMappingOptions
    {
        private static readonly MethodInfo getConfiguratorGenericMethod =
            ReflectionHelper.GetMethodDefinition<TypeMappingOptions>(x => x.GetConfigurator<object>());

        private readonly Type declaringType;
        private readonly List<Type> handlerTypes = new List<Type>();

        private readonly ConcurrentDictionary<string, PropertyMappingOptions> propertyOptions =
            new ConcurrentDictionary<string, PropertyMappingOptions>();

        private readonly List<VirtualPropertyInfo> virtualProperties = new List<VirtualPropertyInfo>();

        private DefaultPropertyInclusionMode defaultPropertyInclusionMode =
            DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;


        public TypeMappingOptions(Type declaringType)
        {
            this.declaringType = declaringType;
        }


        public PropertyInfo ChildToParentProperty { get; set; }
        public ConstructorSpec Constructor { get; private set; }

        public Type DeclaringType
        {
            get { return this.declaringType; }
        }

        public DefaultPropertyInclusionMode DefaultPropertyInclusionMode
        {
            get { return this.defaultPropertyInclusionMode; }
            set { this.defaultPropertyInclusionMode = value; }
        }

        public bool? DeleteAllowed { get; private set; }

        public List<Type> HandlerTypes
        {
            get { return this.handlerTypes; }
        }

        public bool? IsAbstract { get; private set; }
        public bool? IsExposedAsRepository { get; private set; }
        public bool? IsIndependentTypeRoot { get; private set; }
        public bool? IsSingleton { get; private set; }
        public bool? IsUriBaseType { get; private set; }
        public bool? IsValueObject { get; private set; }
        public Action<object> OnDeserialized { get; private set; }
        public PropertyInfo ParentToChildProperty { get; internal set; }
        public bool? PatchAllowed { get; private set; }
        public string PluralName { get; private set; }
        public bool? PostAllowed { get; private set; }
        public Type PostResponseType { get; private set; }
        public string UrlRelativePath { get; private set; }

        public ICollection<VirtualPropertyInfo> VirtualProperties
        {
            get { return this.virtualProperties; }
        }

        internal string Name { get; private set; }


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
            //var propInfo = this.declaringType.GetProperty(name,
            //    BindingFlags.Public | BindingFlags.NonPublic
            //    | (propertyInfo.IsStatic() ? BindingFlags.Static : BindingFlags.Instance));
            //if (propInfo == null)
            //{
            //    throw new InvalidOperationException(
            //        "No property with name " + name + " found on type " + this.declaringType.FullName);
            //}

            //return this.propertyOptions.GetOrAdd(propInfo.Name, pi => new PropertyMappingOptions(propInfo));
            return this.propertyOptions.GetOrAdd(propertyInfo.Name, pi => new PropertyMappingOptions(propertyInfo));
        }


        private PropertyMappingOptions GetPropertyOptions(Expression propertyExpr)
        {
            if (propertyExpr == null)
                throw new ArgumentNullException("propertyExpr");
            var propInfo = propertyExpr.ExtractPropertyInfo();
            var propOptions = this.propertyOptions.GetOrAdd(
                propInfo.Name,
                pi => new PropertyMappingOptions(propInfo));
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


            public override ITypeMappingConfigurator<TDeclaringType> AsAbstract()
            {
                this.owner.IsAbstract = true;
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


            public override ITypeMappingConfigurator<TDeclaringType> AsChildResourceOf<TParent>(
                Expression<Func<TDeclaringType, TParent>> parentProperty,
                Expression<Func<TParent, TDeclaringType>> childProperty)
            {
                if (parentProperty == null)
                    throw new ArgumentNullException("parentProperty");
                if (childProperty == null)
                    throw new ArgumentNullException("childProperty");
                this.owner.ChildToParentProperty = parentProperty.ExtractPropertyInfo();
                this.owner.ParentToChildProperty = childProperty.ExtractPropertyInfo();
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsConcrete()
            {
                this.owner.IsAbstract = false;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsEntity()
            {
                throw new NotImplementedException();
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsIndependentTypeRoot()
            {
                if (IsMappingSubclass())
                    return this;
                this.owner.IsIndependentTypeRoot = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsSingleton()
            {
                this.owner.IsSingleton = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsUriBaseType()
            {
                this.owner.IsUriBaseType = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> AsValueObject()
            {
                this.owner.IsValueObject = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
                Expression<Func<IConstructorControl<TDeclaringType>, TDeclaringType>> constructExpr)
            {
                if (IsMappingSubclass())
                    return this;
                this.owner.Constructor = new ConstructorSpec(constructExpr);
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


            public override ITypeMappingConfigurator<TDeclaringType> DeleteAllowed()
            {
                this.owner.DeleteAllowed = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> DeleteDenied()
            {
                this.owner.DeleteAllowed = false;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> Exclude(
                Expression<Func<TDeclaringType, object>> property)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                var propOptions = this.owner.GetPropertyOptions(property);
                propOptions.InclusionMode = PropertyInclusionMode.Excluded;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> ExposedAsRepository()
            {
                this.owner.IsExposedAsRepository = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> ExposedAt(string path)
            {
                this.owner.UrlRelativePath = path;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> HandledBy<THandler>()
            {
                this.owner.HandlerTypes.Add(typeof(THandler));
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(string name,
                                                                                            Func
                                                                                                <
                                                                                                IPropertyOptionsBuilder
                                                                                                <TDeclaringType, TPropertyType>,
                                                                                                IPropertyOptionsBuilder
                                                                                                <TDeclaringType, TPropertyType>> options)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                var propInfo = VirtualPropertyInfo.Create(name, typeof(TDeclaringType), this.owner.DeclaringType,
                                                          typeof(TPropertyType), PropertyAttributes.None, true, false);
                this.owner.VirtualProperties.Add(propInfo);
                return Include(propInfo, options, propInfo.PropertyType);
            }


            public override ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
                Expression<Func<TDeclaringType, TPropertyType>> property,
                Func
                    <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                var propInfo = property.ExtractPropertyInfo();
                return Include(propInfo, options, propInfo.PropertyType);
            }


            public override ITypeMappingConfigurator<TDeclaringType> IncludeAs<TPropertyType>(
                Expression<Func<TDeclaringType, object>> property,
                Func
                    <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                var propInfo = property.ExtractPropertyInfo();
                return Include(propInfo, options);
            }


            public override ITypeMappingConfigurator<TDeclaringType> Named(string exposedTypeName)
            {
                this.owner.Name = exposedTypeName;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> OnDeserialized(Action<TDeclaringType> action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                this.owner.OnDeserialized = x => action((TDeclaringType)x);
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PatchAllowed()
            {
                this.owner.PatchAllowed = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PatchDenied()
            {
                this.owner.PatchAllowed = false;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostAllowed()
            {
                this.owner.PostAllowed = true;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostDenied()
            {
                this.owner.PostAllowed = false;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostReturns<TPostResponseType>()
            {
                return PostReturns(typeof(TPostResponseType));
            }


            public override ITypeMappingConfigurator<TDeclaringType> PostReturns(Type type)
            {
                this.owner.PostResponseType = type;
                return this;
            }


            public override ITypeMappingConfigurator<TDeclaringType> WithPluralName(string pluralName)
            {
                this.owner.PluralName = pluralName;
                return this;
            }


            protected override ITypeMappingConfigurator<TDeclaringType> OnHasChild<TItem>(
                Expression<Func<TDeclaringType, TItem>> childProperty,
                Expression<Func<TItem, TDeclaringType>> parentProperty,
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
                Func<IPropertyOptionsBuilder<TDeclaringType, TItem>, IPropertyOptionsBuilder<TDeclaringType, TItem>>
                    propertyOptions)
            {
                return Include(childProperty, propertyOptions);
            }


            protected override ITypeMappingConfigurator<TDeclaringType> OnHasChildren<TItem>(
                Expression<Func<TDeclaringType, IEnumerable<TItem>>> property,
                Expression<Func<TItem, TDeclaringType>> parentProperty,
                Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> childConfig,
                Func
                    <IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>,
                    IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>> options)
            {
                return Include(property, options);
            }


            private ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
                PropertyInfo propInfo,
                Func
                    <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null,
                Type propertyType = null)
            {
                var propOptions = this.owner.GetPropertyOptions(propInfo);

                propOptions.InclusionMode = PropertyInclusionMode.Included;
                propOptions.PropertyType = propertyType ?? typeof(TPropertyType);

                if (options != null)
                    options(new PropertyOptionsBuilder<TDeclaringType, TPropertyType>(propOptions));

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