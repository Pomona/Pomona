#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    public class WrappingTypeMappingFilter : ITypeMappingFilter
    {
        private readonly ITypeMappingFilter wrappedFilter;


        public WrappingTypeMappingFilter(ITypeMappingFilter wrappedFilter)
        {
            this.wrappedFilter = wrappedFilter;
        }


        protected virtual ITypeMappingFilter WrappedFilter => this.wrappedFilter;

        public virtual string ApiVersion => this.wrappedFilter.ApiVersion;


        public virtual bool ClientEnumIsGeneratedAsStringEnum(Type enumType)
        {
            return this.wrappedFilter.ClientEnumIsGeneratedAsStringEnum(enumType);
        }


        public ClientMetadata ClientMetadata => this.wrappedFilter.ClientMetadata;


        public virtual bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.ClientPropertyIsExposedAsRepository(propertyInfo);
        }


        public virtual bool DeleteOfTypeIsAllowed(Type type)
        {
            return this.wrappedFilter.DeleteOfTypeIsAllowed(type);
        }


        public virtual bool GenerateIndependentClient()
        {
            return this.wrappedFilter.GenerateIndependentClient();
        }


        public virtual IEnumerable<PropertyInfo> GetAllPropertiesOfType(Type type, BindingFlags bindingFlags)
        {
            return this.wrappedFilter.GetAllPropertiesOfType(type, bindingFlags);
        }


        public virtual PropertyInfo GetChildToParentProperty(Type type)
        {
            return this.wrappedFilter.GetChildToParentProperty(type);
        }


        public IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member)
        {
            return this.wrappedFilter.GetClientLibraryAttributes(member);
        }


        public virtual Type GetClientLibraryType(Type type)
        {
            return this.wrappedFilter.GetClientLibraryType(type);
        }


        public virtual DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return this.wrappedFilter.GetDefaultPropertyInclusionMode();
        }


        public virtual JsonConverter GetJsonConverterForType(Type type)
        {
            return this.wrappedFilter.GetJsonConverterForType(type);
        }


        public virtual Action<object> GetOnDeserializedHook(Type type)
        {
            return this.wrappedFilter.GetOnDeserializedHook(type);
        }


        public virtual PropertyInfo GetParentToChildProperty(Type type)
        {
            return this.wrappedFilter.GetParentToChildProperty(type);
        }


        public virtual string GetPluralNameForType(Type type)
        {
            return this.wrappedFilter.GetPluralNameForType(type);
        }


        public virtual Type GetPostReturnType(Type type)
        {
            return this.wrappedFilter.GetPostReturnType(type);
        }


        public virtual HttpMethod GetPropertyAccessMode(PropertyInfo propertyInfo, ConstructorSpec constructorSpec)
        {
            return this.wrappedFilter.GetPropertyAccessMode(propertyInfo, constructorSpec);
        }


        public virtual IEnumerable<Attribute> GetPropertyAttributes(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyAttributes(type, propertyInfo);
        }


        public virtual PropertyCreateMode GetPropertyCreateMode(Type type,
                                                                PropertyInfo propertyInfo,
                                                                ParameterInfo ctorParameterInfo)
        {
            return this.wrappedFilter.GetPropertyCreateMode(type, propertyInfo, ctorParameterInfo);
        }


        public virtual ExpandMode GetPropertyExpandMode(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyExpandMode(type, propertyInfo);
        }


        public virtual PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyFlags(propertyInfo);
        }


        public virtual LambdaExpression GetPropertyFormula(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyFormula(type, propertyInfo);
        }


        public virtual PropertyGetter GetPropertyGetter(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyGetter(type, propertyInfo);
        }


        public virtual HttpMethod GetPropertyItemAccessMode(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyItemAccessMode(type, propertyInfo);
        }


        public virtual string GetPropertyMappedName(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyMappedName(type, propertyInfo);
        }


        public virtual PropertySetter GetPropertySetter(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertySetter(type, propertyInfo);
        }


        public virtual Type GetPropertyType(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyType(type, propertyInfo);
        }


        public virtual IEnumerable<Type> GetResourceHandlers(Type type)
        {
            return this.wrappedFilter.GetResourceHandlers(type);
        }


        public virtual ConstructorSpec GetTypeConstructor(Type type)
        {
            return this.wrappedFilter.GetTypeConstructor(type);
        }


        public virtual bool GetTypeIsAbstract(Type type)
        {
            return this.wrappedFilter.GetTypeIsAbstract(type);
        }


        public virtual string GetTypeMappedName(Type type)
        {
            return this.wrappedFilter.GetTypeMappedName(type);
        }


        public virtual Type GetUriBaseType(Type type)
        {
            return this.wrappedFilter.GetUriBaseType(type);
        }


        public virtual string GetUrlRelativePath(Type type)
        {
            return this.wrappedFilter.GetUrlRelativePath(type);
        }


        public virtual bool IsIndependentTypeRoot(Type type)
        {
            return this.wrappedFilter.IsIndependentTypeRoot(type);
        }


        public virtual bool PatchOfTypeIsAllowed(Type type)
        {
            return this.wrappedFilter.PatchOfTypeIsAllowed(type);
        }


        public virtual bool PostOfTypeIsAllowed(Type type)
        {
            return this.wrappedFilter.PostOfTypeIsAllowed(type);
        }


        public virtual bool PropertyIsAttributes(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsAttributes(type, propertyInfo);
        }


        public virtual bool PropertyIsEtag(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsEtag(type, propertyInfo);
        }


        public virtual bool PropertyIsIncluded(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsIncluded(type, propertyInfo);
        }


        public virtual bool PropertyIsPrimaryId(Type type, PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsPrimaryId(type, propertyInfo);
        }


        public virtual Type ResolveRealTypeForProxy(Type type)
        {
            return this.wrappedFilter.ResolveRealTypeForProxy(type);
        }


        public virtual bool TypeIsExposedAsRepository(Type type)
        {
            return this.wrappedFilter.TypeIsExposedAsRepository(type);
        }


        public virtual bool TypeIsMapped(Type type)
        {
            return this.wrappedFilter.TypeIsMapped(type);
        }


        public virtual bool TypeIsMappedAsCollection(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsCollection(type);
        }


        public virtual bool TypeIsMappedAsSharedType(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsSharedType(type);
        }


        public virtual bool TypeIsMappedAsTransformedType(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsTransformedType(type);
        }


        public virtual bool TypeIsMappedAsValueObject(Type type)
        {
            return this.wrappedFilter.TypeIsMappedAsValueObject(type);
        }


        public virtual bool TypeIsSingletonResource(Type type)
        {
            return this.wrappedFilter.TypeIsSingletonResource(type);
        }
    }
}