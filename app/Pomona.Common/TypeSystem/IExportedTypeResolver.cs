using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public interface IExportedTypeResolver : ITypeResolver
    {
        IEnumerable<TransformedType> GetAllTransformedTypes();
        ExportedPropertyDetails LoadExportedPropertyDetails(PropertyMapping propertyMapping);
        ExportedTypeDetails LoadExportedTypeDetails(TransformedType exportedType);
        ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
    }
}