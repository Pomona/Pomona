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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.Serialization;
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


        public virtual string ApiVersion
        {
            get { return this.wrappedFilter.ApiVersion; }
        }

        protected virtual ITypeMappingFilter WrappedFilter
        {
            get { return this.wrappedFilter; }
        }


        public virtual bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.ClientPropertyIsExposedAsRepository(propertyInfo);
        }


        public IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member)
        {
            return this.wrappedFilter.GetClientLibraryAttributes(member);
        }


        public virtual bool GenerateIndependentClient()
        {
            return this.wrappedFilter.GenerateIndependentClient();
        }


        public virtual PropertyInfo GetChildToParentProperty(Type type)
        {
            return this.wrappedFilter.GetChildToParentProperty(type);
        }


        public virtual string GetClientAssemblyName()
        {
            return this.wrappedFilter.GetClientAssemblyName();
        }


        public virtual string GetClientInformationalVersion()
        {
            return this.wrappedFilter.GetClientInformationalVersion();
        }


        public virtual Type GetClientLibraryType(Type type)
        {
            return this.wrappedFilter.GetClientLibraryType(type);
        }


        public virtual LambdaExpression GetDecompiledPropertyFormula(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetDecompiledPropertyFormula(propertyInfo);
        }


        public virtual DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return this.wrappedFilter.GetDefaultPropertyInclusionMode();
        }


        public virtual JsonConverter GetJsonConverterForType(Type type)
        {
            return this.wrappedFilter.GetJsonConverterForType(type);
        }


        public virtual bool DeleteOfTypeIsAllowed(Type type)
        {
            return this.wrappedFilter.DeleteOfTypeIsAllowed(type);
        }


        public virtual Action<object> GetOnDeserializedHook(Type type)
        {
            return this.wrappedFilter.GetOnDeserializedHook(type);
        }


        public virtual PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty)
        {
            return this.wrappedFilter.GetOneToManyCollectionForeignKey(collectionProperty);
        }


        public virtual PropertyInfo GetParentToChildProperty(Type type)
        {
            return this.wrappedFilter.GetParentToChildProperty(type);
        }


        public virtual string GetTypeMappedName(Type type)
        {
            return this.wrappedFilter.GetTypeMappedName(type);
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


        public virtual PropertyCreateMode GetPropertyCreateMode(PropertyInfo propertyInfo,
            ParameterInfo ctorParameterInfo)
        {
            return this.wrappedFilter.GetPropertyCreateMode(propertyInfo, ctorParameterInfo);
        }


        public virtual PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyFlags(propertyInfo);
        }


        public virtual IEnumerable<Type> GetResourceHandlers(Type type)
        {
            return this.wrappedFilter.GetResourceHandlers(type);
        }

        public virtual bool GetTypeIsAbstract(Type type)
        {
            return this.wrappedFilter.GetTypeIsAbstract(type);
        }


        public virtual LambdaExpression GetPropertyFormula(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyFormula(propertyInfo);
        }


        public virtual Func<object, IContextResolver, object> GetPropertyGetter(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyGetter(propertyInfo);
        }


        public virtual HttpMethod GetPropertyItemAccessMode(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyItemAccessMode(propertyInfo);
        }


        public virtual string GetPropertyMappedName(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyMappedName(propertyInfo);
        }


        public virtual Action<object, object, IContextResolver> GetPropertySetter(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertySetter(propertyInfo);
        }


        public virtual Type GetPropertyType(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.GetPropertyType(propertyInfo);
        }


        public virtual ConstructorSpec GetTypeConstructor(Type type)
        {
            return this.wrappedFilter.GetTypeConstructor(type);
        }


        public virtual Type GetUriBaseType(Type type)
        {
            return this.wrappedFilter.GetUriBaseType(type);
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


        public virtual bool PropertyFormulaIsDecompiled(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyFormulaIsDecompiled(propertyInfo);
        }


        public virtual bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsAlwaysExpanded(propertyInfo);
        }


        public virtual bool PropertyIsAttributes(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsAttributes(propertyInfo);
        }


        public virtual bool PropertyIsEtag(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsEtag(propertyInfo);
        }


        public virtual bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsIncluded(propertyInfo);
        }


        public virtual bool PropertyIsPrimaryId(PropertyInfo propertyInfo)
        {
            return this.wrappedFilter.PropertyIsPrimaryId(propertyInfo);
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
    }
}