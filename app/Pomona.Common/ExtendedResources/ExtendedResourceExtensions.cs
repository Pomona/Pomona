#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    public static class ExtendedResourceExtensions
    {
        public static TOriginal Unwrap<TOriginal>(this IClientResource wrapped)
            where TOriginal : class, IClientResource
        {
            var er = wrapped as IExtendedResourceProxy;
            if (er == null)
                throw new ArgumentException("Can only unwrap a wrapped resource.", nameof(wrapped));
            var unwrapped = er.WrappedResource as TOriginal;
            if (unwrapped == null)
                throw new ArgumentException("Unable to unwrap to type " + typeof(TOriginal), nameof(wrapped));
            return unwrapped;
        }


        public static TExtended Wrap<TOriginal, TExtended>(this TOriginal resource)
            where TOriginal : IClientResource
            where TExtended : TOriginal, IClientResource
        {
            var typeMapper = ClientTypeMapper.GetTypeMapper(typeof(TOriginal));
            return (TExtended)typeMapper.WrapResource(resource, typeof(TOriginal), typeof(TExtended));
        }
    }
}