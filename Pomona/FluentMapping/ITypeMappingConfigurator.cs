using System;
using System.Linq.Expressions;

namespace Pomona.FluentMapping
{
    public interface ITypeMappingConfigurator<TDeclaringType>
    {
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreExcludedByDefault();
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreIncludedByDefault();
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesRequiresExplicitMapping();
        ITypeMappingConfigurator<TDeclaringType> AsEntity();
        ITypeMappingConfigurator<TDeclaringType> AsUriBaseType();
        ITypeMappingConfigurator<TDeclaringType> AsValueObject();

        ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
            Expression<Func<TDeclaringType, TDeclaringType>> constructExpr);

        ITypeMappingConfigurator<TDeclaringType> Exclude(Expression<Func<TDeclaringType, object>> property);


        ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
            Expression<Func<TDeclaringType, TPropertyType>> property,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null);


        ITypeMappingConfigurator<TDeclaringType> PostReturns<TPostResponseType>();
        ITypeMappingConfigurator<TDeclaringType> PostReturns(Type postResponseType);
    }
}