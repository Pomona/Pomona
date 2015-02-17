using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public interface IExportedTypeResolver : ITypeResolver
    {
        IEnumerable<ComplexType> GetAllComplexTypes();
        ComplexPropertyDetails LoadComplexPropertyDetails(ComplexProperty complexProperty);
        ComplexTypeDetails LoadComplexTypeDetails(ComplexType exportedType);
        ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
    }
}