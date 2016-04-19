#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.ExtendedResources;

namespace Pomona.Common.Proxies
{
    public interface IExtendedResourceProxy<TWrappedResource> : IExtendedResourceProxy
    {
    }

    public interface IExtendedResourceProxy
    {
        ExtendedResourceInfo UserTypeInfo { get; }
        object WrappedResource { get; }
    }
}