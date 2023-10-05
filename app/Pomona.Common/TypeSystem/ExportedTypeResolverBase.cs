#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public abstract class ExportedTypeResolverBase : TypeResolver, IResourceTypeResolver
    {
        public override IEnumerable<PropertySpec> LoadRequiredProperties(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            var transformedType = typeSpec as StructuredType;
            if (transformedType != null)
            {
                if (!transformedType.PostAllowed)
                    return Enumerable.Empty<PropertySpec>();
                if (transformedType.Constructor != null)
                {
                    IEnumerable<PropertySpec> requiredProperties =
                        transformedType.Constructor.ParameterSpecs.Where(x => x.IsRequired).Select(
                            x => typeSpec.GetPropertyByName(x.PropertyInfo.Name, true)).ToList();
                    return
                        requiredProperties;
                }
            }

            return base.LoadRequiredProperties(typeSpec);
        }


        public abstract ResourcePropertyDetails LoadResourcePropertyDetails(ResourceProperty property);
        public abstract ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
        public abstract StructuredPropertyDetails LoadStructuredPropertyDetails(StructuredProperty property);
        public abstract StructuredTypeDetails LoadStructuredTypeDetails(StructuredType structuredType);
        public abstract IEnumerable<StructuredType> LoadSubTypes(StructuredType baseType);
    }
}

