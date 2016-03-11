#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.TypeSystem
{
    public class StructuredPropertyDetails
    {
        public StructuredPropertyDetails(bool isAttributesProperty,
                                         bool isEtagProperty,
                                         bool isPrimaryKey,
                                         bool isSerialized,
                                         HttpMethod accessMode,
                                         HttpMethod itemAccessMode,
                                         ExpandMode expandMode)
        {
            IsAttributesProperty = isAttributesProperty;
            IsEtagProperty = isEtagProperty;
            IsPrimaryKey = isPrimaryKey;
            IsSerialized = isSerialized;
            AccessMode = accessMode;
            ItemAccessMode = itemAccessMode;
            ExpandMode = expandMode;
        }


        public HttpMethod AccessMode { get; }

        public ExpandMode ExpandMode { get; }

        public bool IsAttributesProperty { get; }

        public bool IsEtagProperty { get; }

        public bool IsPrimaryKey { get; }

        public bool IsSerialized { get; }

        public HttpMethod ItemAccessMode { get; }
    }
}