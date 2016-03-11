#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal class ExtendedComplexOverlayProperty : ExtendedOverlayProperty
    {
        public ExtendedComplexOverlayProperty(PropertyInfo property,
                                              PropertyInfo underlyingProperty,
                                              ExtendedResourceInfo info)
            : base(property, underlyingProperty, info)
        {
        }


        public override object GetValue(object obj, IDictionary<string, IExtendedResourceProxy> cache)
        {
            IExtendedResourceProxy extendedResource;
            var underlyingResource = UnderlyingProperty.GetValue(obj, null);

            if (TryGetFromCache(cache, underlyingResource, out extendedResource))
                return extendedResource;

            if (underlyingResource != null)
            {
                extendedResource =
                    (IExtendedResourceProxy)
                        RuntimeProxyFactory.Create(typeof(ExtendedResourceBase), Property.PropertyType);
                var proxyBase = (ExtendedResourceBase)extendedResource;
                proxyBase.Initialize(ClientTypeResolver.Default, Info, underlyingResource);
            }
            else
                extendedResource = null;
            cache[Property.Name] = extendedResource;
            return extendedResource;
        }


        public override void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache)
        {
            if (value == null)
            {
                cache.Remove(Property.Name);
                UnderlyingProperty.SetValue(obj, null, null);
                return;
            }
            var extendedResource = (IExtendedResourceProxy)value;
            var underlyingResource = extendedResource.WrappedResource;
            UnderlyingProperty.SetValue(obj, underlyingResource, null);
            cache[Property.Name] = extendedResource;
        }
    }
}