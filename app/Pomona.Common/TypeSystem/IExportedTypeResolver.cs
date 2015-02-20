using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public interface IExportedTypeResolver : ITypeResolver
    {
        IEnumerable<StructuredType> GetAllStructuredTypes();
        StructuredPropertyDetails LoadStructuredPropertyDetails(StructuredProperty property);
        StructuredTypeDetails LoadStructuredTypeDetails(StructuredType structuredType);
        ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
    }
}