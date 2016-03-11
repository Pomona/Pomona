#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal abstract class ExtendedOverlayProperty : ExtendedProperty
    {
        protected ExtendedOverlayProperty(PropertyInfo property,
                                          PropertyInfo underlyingProperty,
                                          ExtendedResourceInfo info)
            : base(property)
        {
            UnderlyingProperty = underlyingProperty;
            Info = info;
        }


        public ExtendedResourceInfo Info { get; }

        public PropertyInfo UnderlyingProperty { get; }


        protected bool TryGetFromCache(IDictionary<string, IExtendedResourceProxy> cache,
                                       object underlyingResource,
                                       out IExtendedResourceProxy extendedResource)
        {
            if (cache.TryGetValue(Property.Name, out extendedResource))
            {
                if ((extendedResource != null && extendedResource.WrappedResource == underlyingResource) || underlyingResource == null)
                    return true;
            }
            return false;
        }
    }
}