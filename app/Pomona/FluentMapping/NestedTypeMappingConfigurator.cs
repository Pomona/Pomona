#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona.FluentMapping
{
    internal class NestedTypeMappingConfigurator<TDeclaring> : TypeMappingConfiguratorBase<TDeclaring>
    {
        private readonly List<Delegate> typeConfigurationDelegates = new List<Delegate>();


        public NestedTypeMappingConfigurator(List<Delegate> typeConfigurationDelegates)
        {
            this.typeConfigurationDelegates = typeConfigurationDelegates;
        }


        protected override ITypeMappingConfigurator<TDeclaring> OnHasChild<TItem>(
            Expression<Func<TDeclaring, TItem>> childProperty,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func<IPropertyOptionsBuilder<TDeclaring, TItem>, IPropertyOptionsBuilder<TDeclaring, TItem>>
                propertyOptions)
        {
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping =
                x => x.AsChildResourceOf(parentProperty, childProperty);
            this.typeConfigurationDelegates.Add(asChildResourceMapping);
            this.typeConfigurationDelegates.Add(typeOptions);
            return this;
        }


        protected override ITypeMappingConfigurator<TDeclaring> OnHasChildren<TItem>(
            Expression<Func<TDeclaring, IEnumerable<TItem>>> collectionProperty,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func
                <IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>,
                IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
        {
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping =
                x => x.AsChildResourceOf(parentProperty, collectionProperty);
            this.typeConfigurationDelegates.Add(asChildResourceMapping);
            this.typeConfigurationDelegates.Add(typeOptions);
            return this;
        }
    }
}