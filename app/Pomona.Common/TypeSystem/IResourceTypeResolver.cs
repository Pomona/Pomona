#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.TypeSystem
{
    public interface IResourceTypeResolver : IStructuredTypeResolver
    {
        ResourcePropertyDetails LoadResourcePropertyDetails(ResourceProperty property);
        ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);
    }
}