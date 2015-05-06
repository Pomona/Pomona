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
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;
using Pomona.Mapping;

namespace Pomona
{
    public interface ITypeMappingFilter : ITypeConventions, IPropertyConventions, IResourceConventions
    {
        #region ApiMetadataConfiguration

        string ApiVersion { get; }

        #endregion

        #region ProxyTypeResolver

        Type ResolveRealTypeForProxy(Type type);

        #endregion

        #region Undecided stuff:

        DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode();
        JsonConverter GetJsonConverterForType(Type type);
        Action<object> GetOnDeserializedHook(Type type);

        #endregion

        #region Access rules, don't know where to put this yet

        bool DeleteOfTypeIsAllowed(Type type);
        HttpMethod GetPropertyAccessMode(PropertyInfo propertyInfo, ConstructorSpec constructorSpec);
        PropertyCreateMode GetPropertyCreateMode(Type type, PropertyInfo propertyInfo, ParameterInfo ctorParameterInfo);
        PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo);
        HttpMethod GetPropertyItemAccessMode(Type type, PropertyInfo propertyInfo);
        bool PatchOfTypeIsAllowed(Type type);
        bool PostOfTypeIsAllowed(Type type);

        #endregion

        #region GeneratedClientConfiguration

        /// <summary>
        /// Gets the metadata about the generated REST client.
        /// </summary>
        /// <returns>
        /// The the metadata about the generated REST client.
        /// </returns>
        ClientMetadata ClientMetadata { get; }


        /// <summary>
        /// This will make sure we generate a client dll with no dependency on Pomona.Common.
        /// </summary>
        bool GenerateIndependentClient();

        #endregion

        #region GeneratedClientConventions

        // NOTE: This should probably take a TypeSpec not a clr Type
        bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo);
        bool ClientEnumIsGeneratedAsStringEnum(Type enumType);
        IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member);
        Type GetClientLibraryType(Type type);

        #endregion

        #region Property mapping conventions

        bool PropertyIsPrimaryId(Type type, PropertyInfo propertyInfo);
        bool PropertyIsAttributes(Type type, PropertyInfo propertyInfo);
        bool PropertyIsEtag(Type type, PropertyInfo propertyInfo);

        #endregion
    }
}