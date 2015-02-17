using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public interface IExportedTypeResolver : ITypeResolver
    {
        IEnumerable<ComplexType> GetAllTransformedTypes();
        ComplexPropertyDetails LoadComplexPropertyDetails(ComplexProperty complexProperty);
        ComplexTypeDetails LoadComplexTypeDetails(ComplexType exportedType);
        ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
    }
}