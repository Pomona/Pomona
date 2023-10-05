#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal abstract class TypeMappingConfiguratorBase<TDeclaring> : ITypeMappingConfigurator<TDeclaring>
    {
        protected virtual ITypeMappingConfigurator<TDeclaring> OnHasChild<TItem>(Expression<Func<TDeclaring, TItem>> childProperty,
                                                                                 Expression<Func<TItem, TDeclaring>> parentProperty,
                                                                                 Func
                                                                                     <ITypeMappingConfigurator<TItem>,
                                                                                     ITypeMappingConfigurator<TItem>> typeOptions,
                                                                                 Func
                                                                                     <IPropertyOptionsBuilder<TDeclaring, TItem>,
                                                                                     IPropertyOptionsBuilder<TDeclaring, TItem>>
                                                                                     propertyOptions)
        {
            return this;
        }


        protected virtual ITypeMappingConfigurator<TDeclaring> OnHasChildren<TItem>(
            Expression<Func<TDeclaring, IEnumerable<TItem>>> property,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func
                <IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>,
                IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AllPropertiesAreExcludedByDefault()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AllPropertiesAreIncludedByDefault()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AllPropertiesRequiresExplicitMapping()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsAbstract()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsChildResourceOf<TParent>(
            Expression<Func<TDeclaring, TParent>> parentProperty,
            Expression<Func<TParent, IEnumerable<TDeclaring>>> collectionProperty)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsChildResourceOf<TParent>(Expression<Func<TDeclaring, TParent>> parentProperty,
                                                                                       Expression<Func<TParent, TDeclaring>> childProperty)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsConcrete()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsEntity()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsIndependentTypeRoot()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsSingleton()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsUriBaseType()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsValueObject()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ConstructedUsing(
            Expression<Func<IConstructorControl<TDeclaring>, TDeclaring>> constructExpr)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ConstructedUsing(
            Expression<Func<TDeclaring, IConstructorControl<TDeclaring>, TDeclaring>> constructExpr)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> DeleteAllowed()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> DeleteDenied()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Exclude(Expression<Func<TDeclaring, object>> property)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ExposedAsRepository()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ExposedAt(string path)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> HandledBy<THandler>()
        {
            return this;
        }


        public ITypeMappingConfigurator<TDeclaring> HasChild<TItem>(Expression<Func<TDeclaring, TItem>> childProperty,
                                                                    Expression<Func<TItem, TDeclaring>> parentProperty,
                                                                    Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>>
                                                                        typeOptions = null,
                                                                    Func
                                                                        <IPropertyOptionsBuilder<TDeclaring, TItem>,
                                                                        IPropertyOptionsBuilder<TDeclaring, TItem>> propertyOptions = null)
        {
            if (childProperty == null)
                throw new ArgumentNullException(nameof(childProperty));
            if (parentProperty == null)
                throw new ArgumentNullException(nameof(parentProperty));
            return OnHasChild(childProperty, parentProperty, typeOptions ?? (x => x), propertyOptions ?? (x => x));
        }


        public ITypeMappingConfigurator<TDeclaring> HasChildren<TItem>(
            Expression<Func<TDeclaring, IEnumerable<TItem>>> collectionProperty,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func
                <IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>,
                IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
        {
            if (collectionProperty == null)
                throw new ArgumentNullException(nameof(collectionProperty));
            if (parentProperty == null)
                throw new ArgumentNullException(nameof(parentProperty));
            return OnHasChildren(collectionProperty, parentProperty, typeOptions ?? (x => x), propertyOptions ?? (x => x));
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Include<TPropertyType>(string name,
                                                                                   Func
                                                                                       <IPropertyOptionsBuilder<TDeclaring, TPropertyType>,
                                                                                       IPropertyOptionsBuilder<TDeclaring, TPropertyType>>
                                                                                       options)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Include<TPropertyType>(
            Expression<Func<TDeclaring, TPropertyType>> property,
            Func<IPropertyOptionsBuilder<TDeclaring, TPropertyType>, IPropertyOptionsBuilder<TDeclaring, TPropertyType>>
                options = null)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> IncludeAs<TPropertyType>(Expression<Func<TDeclaring, object>> property,
                                                                                     Func
                                                                                         <IPropertyOptionsBuilder<TDeclaring, TPropertyType>,
                                                                                         IPropertyOptionsBuilder<TDeclaring, TPropertyType>>
                                                                                         options = null)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Named(string exposedTypeName)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> OnDeserialized(Action<TDeclaring> action)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PatchAllowed()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PatchDenied()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostAllowed()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostDenied()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostReturns<TPostResponseType>()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostReturns(Type postResponseType)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> WithPluralName(string pluralName)
        {
            return this;
        }
    }
}
