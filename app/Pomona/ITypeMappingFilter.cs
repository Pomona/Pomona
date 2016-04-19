#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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