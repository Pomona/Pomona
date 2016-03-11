#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal class ExtendedCollectionOverlayProperty : ExtendedOverlayProperty
    {
        private static readonly MethodInfo createProxyListMethod =
            ReflectionHelper.GetMethodDefinition(() => CreateProxyList<IClientResource, IClientResource>(null, null));


        public ExtendedCollectionOverlayProperty(PropertyInfo property,
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
                        createProxyListMethod.MakeGenericMethod(Info.ExtendedType, Info.ServerType).Invoke(null,
                                                                                                           new object[]
                                                                                                           {
                                                                                                               underlyingResource,
                                                                                                               Info
                                                                                                           });
            }
            else
                extendedResource = null;
            cache[Property.Name] = extendedResource;
            return extendedResource;
        }


        public override void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache)
        {
            throw new NotImplementedException();
        }


        private static IList<TExtended> CreateProxyList<TExtended, TServer>(IEnumerable source,
                                                                            ExtendedResourceInfo userTypeInfo)
            where TExtended : TServer, IClientResource
            where TServer : IClientResource
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else

            return new ExtendedResourceList<TExtended, TServer>((IList<TServer>)source, userTypeInfo);
#endif
        }
    }
}