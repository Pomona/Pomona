using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public interface IExportedTypeResolver : ITypeResolver
    {
        IEnumerable<ComplexType> GetAllTransformedTypes();
        ComplexPropertyDetails LoadComplexPropertyDetails(ComplexProperty complexProperty);
        ExportedTypeDetails LoadExportedTypeDetails(ComplexType exportedType);
        ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
    }
}