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
using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;

namespace Pomona
{
    public interface ITypeMappingFilter
    {
        string ApiVersion { get; }
        DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode();
        bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo);
        string GetClientAssemblyName();
        Type GetClientLibraryType(Type type);
        bool IsIndependentTypeRoot(Type type);
        object GetIdFor(object entity);
        JsonConverter GetJsonConverterForType(Type type);
        PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty);
        Type GetPostReturnType(Type type);
        Func<object, object> GetPropertyGetter(PropertyInfo propertyInfo);
        string GetPropertyMappedName(PropertyInfo propertyInfo);
        Action<object, object> GetPropertySetter(PropertyInfo propertyInfo);
        Type GetPropertyType(PropertyInfo propertyInfo);


        /// <summary>
        /// Gets a list of all types to CONSIDER for inclusion.
        /// (they will be filtered first)
        /// </summary>
        /// <returns>List of types considered for mapping.</returns>
        IEnumerable<Type> GetSourceTypes();


        ConstructorInfo GetTypeConstructor(Type type);


        /// <summary>
        /// This returns what URI this type will be mapped to.
        /// For example if this method returns the type Animal when passed Dog
        /// it means that dogs will be available on same url as Animal.
        /// (ie. http://somehost/animal/{id}, not http://somehost/dog/{id})
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Type GetUriBaseType(Type type);


        PropertyInfo GetParentToChildProperty(Type type);
        PropertyInfo GetChildToParentProperty(Type type);

        bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo);

        bool PropertyIsIncluded(PropertyInfo propertyInfo);
        bool PropertyIsPrimaryId(PropertyInfo propertyInfo);
        Type ResolveRealTypeForProxy(Type type);
        bool TypeIsMapped(Type type);
        bool TypeIsMappedAsCollection(Type type);
        bool TypeIsMappedAsSharedType(Type type);
        bool TypeIsMappedAsTransformedType(Type type);
        bool TypeIsMappedAsValueObject(Type type);
        bool TypeIsExposedAsRepository(Type type);
        bool PropertyIsAttributes(PropertyInfo propertyInfo);
        LambdaExpression GetPropertyFormula(PropertyInfo propertyInfo);
        bool PropertyFormulaIsDecompiled(PropertyInfo propertyInfo);
        LambdaExpression GetDecompiledPropertyFormula(PropertyInfo propertyInfo);
        bool PropertyIsEtag(PropertyInfo propertyInfo);
        string GetPluralNameForType(Type type);
        PropertyCreateMode GetPropertyCreateMode(PropertyInfo propertyInfo, ParameterInfo ctorParameterInfo);
        HttpMethod GetPropertyAccessMode(PropertyInfo propertyInfo);
        int? GetPropertyConstructorArgIndex(PropertyInfo propertyInfo);

        bool PostOfTypeIsAllowed(Type type);
        bool PatchOfTypeIsAllowed(Type type);

        Action<object> GetOnDeserializedHook(Type type);
        HttpMethod GetPropertyItemAccessMode(PropertyInfo propertyInfo);
    }
}