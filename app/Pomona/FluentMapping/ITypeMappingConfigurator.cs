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
    public interface ITypeMappingConfigurator<TDeclaringType>
    {
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreExcludedByDefault();
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreIncludedByDefault();
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesRequiresExplicitMapping();
        ITypeMappingConfigurator<TDeclaringType> AsAbstract();


        ITypeMappingConfigurator<TDeclaringType> AsChildResourceOf<TParent>(
            Expression<Func<TDeclaringType, TParent>> parentProperty,
            Expression<Func<TParent, IEnumerable<TDeclaringType>>> collectionProperty);


        ITypeMappingConfigurator<TDeclaringType> AsChildResourceOf<TParent>(
            Expression<Func<TDeclaringType, TParent>> parentProperty,
            Expression<Func<TParent, TDeclaringType>> childProperty);


        ITypeMappingConfigurator<TDeclaringType> AsConcrete();
        ITypeMappingConfigurator<TDeclaringType> AsEntity();
        ITypeMappingConfigurator<TDeclaringType> AsIndependentTypeRoot();
        ITypeMappingConfigurator<TDeclaringType> AsSingleton();
        ITypeMappingConfigurator<TDeclaringType> AsUriBaseType();
        ITypeMappingConfigurator<TDeclaringType> AsValueObject();


        ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
            Expression<Func<IConstructorControl<TDeclaringType>, TDeclaringType>> constructExpr);


        ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
            Expression<Func<TDeclaringType, IConstructorControl<TDeclaringType>, TDeclaringType>> constructExpr);


        ITypeMappingConfigurator<TDeclaringType> DeleteAllowed();
        ITypeMappingConfigurator<TDeclaringType> DeleteDenied();
        ITypeMappingConfigurator<TDeclaringType> Exclude(Expression<Func<TDeclaringType, object>> property);
        ITypeMappingConfigurator<TDeclaringType> ExposedAsRepository();
        ITypeMappingConfigurator<TDeclaringType> ExposedAt(string path);
        ITypeMappingConfigurator<TDeclaringType> HandledBy<THandler>();


        ITypeMappingConfigurator<TDeclaringType> HasChild<TItem>(
            Expression<Func<TDeclaringType, TItem>> childProperty,
            Expression<Func<TItem, TDeclaringType>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions = null,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TItem>,
                IPropertyOptionsBuilder<TDeclaringType, TItem>> propertyOptions = null);


        ITypeMappingConfigurator<TDeclaringType> HasChildren<TItem>(
            Expression<Func<TDeclaringType, IEnumerable<TItem>>> collectionProperty,
            Expression<Func<TItem, TDeclaringType>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions = null,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>,
                IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>> propertyOptions = null);


        ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(string name,
                                                                        Func
                                                                            <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                                                                            IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options);


        ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
            Expression<Func<TDeclaringType, TPropertyType>> property,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null);


        ITypeMappingConfigurator<TDeclaringType> IncludeAs<TPropertyType>(
            Expression<Func<TDeclaringType, object>> property,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null);


        ITypeMappingConfigurator<TDeclaringType> Named(string exposedTypeName);
        ITypeMappingConfigurator<TDeclaringType> OnDeserialized(Action<TDeclaringType> action);
        ITypeMappingConfigurator<TDeclaringType> PatchAllowed();
        ITypeMappingConfigurator<TDeclaringType> PatchDenied();
        ITypeMappingConfigurator<TDeclaringType> PostAllowed();
        ITypeMappingConfigurator<TDeclaringType> PostDenied();
        ITypeMappingConfigurator<TDeclaringType> PostReturns<TPostResponseType>();
        ITypeMappingConfigurator<TDeclaringType> PostReturns(Type postResponseType);
        ITypeMappingConfigurator<TDeclaringType> WithPluralName(string pluralName);
    }
}
