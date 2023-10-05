#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

namespace Pomona.Common.ExtendedResources
{
    internal class ExtendedQueryableRoot<T> : ExtendedQueryable<T>, IExtendedQueryableRoot
    {
        internal ExtendedQueryableRoot(IClientTypeResolver client,
                                       IQueryable wrappedSource,
                                       ExtendedResourceInfo extendedResourceInfo,
                                       ExtendedResourceMapper extendedResourceMapper)
            : base(new ExtendedQueryProvider(extendedResourceMapper), null)
        {
            WrappedSource = wrappedSource;
            ExtendedResourceInfo = extendedResourceInfo;
        }


        public ExtendedResourceInfo ExtendedResourceInfo { get; }

        public IQueryable WrappedSource { get; }
    }
}

