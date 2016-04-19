#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public interface IStructuredTypeResolver : ITypeResolver
    {
        StructuredPropertyDetails LoadStructuredPropertyDetails(StructuredProperty property);
        StructuredTypeDetails LoadStructuredTypeDetails(StructuredType structuredType);
        IEnumerable<StructuredType> LoadSubTypes(StructuredType baseType);
    }
}