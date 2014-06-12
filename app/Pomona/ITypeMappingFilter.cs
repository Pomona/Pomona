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
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;

namespace Pomona
{
    public interface ITypeMappingFilter
    {
        #region Undecided stuff:
        JsonConverter GetJsonConverterForType(Type type);
        bool TypeIsMapped(Type type);
        bool TypeIsMappedAsCollection(Type type);
        bool TypeIsMappedAsSharedType(Type type);
        bool TypeIsMappedAsTransformedType(Type type);
        bool TypeIsMappedAsValueObject(Type type);
        Action<object> GetOnDeserializedHook(Type type);
        DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode();
        #endregion

        #region Delegate decompiler related methods
        // The methods below should maybe be provided in an addon, to remove dependency on DelegateCompiler
        bool PropertyFormulaIsDecompiled(Type type, PropertyInfo propertyInfo);
        LambdaExpression GetDecompiledPropertyFormula(Type type, PropertyInfo propertyInfo);
        #endregion


        #region Access rules, don't know where to put this yet
        PropertyCreateMode GetPropertyCreateMode(Type type, PropertyInfo propertyInfo, ParameterInfo ctorParameterInfo);
        HttpMethod GetPropertyAccessMode(PropertyInfo propertyInfo, ConstructorSpec constructorSpec);
        bool PostOfTypeIsAllowed(Type type);
        bool PatchOfTypeIsAllowed(Type type);
        bool DeleteOfTypeIsAllowed(Type type);
        HttpMethod GetPropertyItemAccessMode(Type type, PropertyInfo propertyInfo);
        PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo);
        #endregion

        #region ApiMetadataConfiguration
        string ApiVersion { get; }
        #endregion


        #region GeneratedClientConfiguration
        string GetClientAssemblyName(); // Maybe expose as property ClientAssemblyName instead
        string GetClientInformationalVersion(); // Expose as property ClientInformationalVersion instead
        /// <summary>
        /// This will make sure we generate a client dll with no dependency on Pomona.Common.
        /// </summary>
        bool GenerateIndependentClient();
        #endregion

        #region GeneratedClientConventions
        Type GetClientLibraryType(Type type); // NOTE: This should probably take a TypeSpec not a clr Type
        bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo);
        IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member);
        #endregion

        #region TypeMappingConventions
        bool IsIndependentTypeRoot(Type type);
        Type GetPostReturnType(Type type);
        ConstructorSpec GetTypeConstructor(Type type);
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
        Type ResolveRealTypeForProxy(Type type);
        bool TypeIsExposedAsRepository(Type type);
        string GetTypeMappedName(Type type);
        string GetPluralNameForType(Type type);
        IEnumerable<Type> GetResourceHandlers(Type type);
        bool GetTypeIsAbstract(Type type);
        #endregion

        #region Property mapping conventions
        bool PropertyIsAttributes(Type type, PropertyInfo propertyInfo);
        LambdaExpression GetPropertyFormula(Type type, PropertyInfo propertyInfo);
        bool PropertyIsAlwaysExpanded(Type type, PropertyInfo propertyInfo);
        bool PropertyIsIncluded(Type type, PropertyInfo propertyInfo);
        bool PropertyIsPrimaryId(Type type, PropertyInfo propertyInfo);
        bool PropertyIsEtag(Type type, PropertyInfo propertyInfo);
        Func<object, IContextResolver, object> GetPropertyGetter(Type type, PropertyInfo propertyInfo);
        Action<object, object, IContextResolver> GetPropertySetter(Type type, PropertyInfo propertyInfo);
        string GetPropertyMappedName(Type type, PropertyInfo propertyInfo);
        Type GetPropertyType(Type type, PropertyInfo propertyInfo);
        #endregion
    }
}