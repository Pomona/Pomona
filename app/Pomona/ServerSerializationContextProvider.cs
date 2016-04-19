#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ServerSerializationContextProvider : ISerializationContextProvider
    {
        private readonly IContainer container;
        private readonly IResourceResolver resourceResolver;
        private readonly ITypeResolver typeMapper;
        private readonly IUriResolver uriResolver;


        public ServerSerializationContextProvider(ITypeResolver typeMapper,
                                                  IUriResolver uriResolver,
                                                  IResourceResolver resourceResolver,
                                                  IContainer container)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            if (uriResolver == null)
                throw new ArgumentNullException(nameof(uriResolver));
            if (resourceResolver == null)
                throw new ArgumentNullException(nameof(resourceResolver));
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            this.typeMapper = typeMapper;
            this.uriResolver = uriResolver;
            this.resourceResolver = resourceResolver;
            this.container = container;
        }


        public IDeserializationContext GetDeserializationContext(DeserializeOptions options)
        {
            options = options ?? new DeserializeOptions();
            return new ServerDeserializationContext(this.typeMapper, this.resourceResolver, options.TargetNode, this.container);
        }


        public ISerializationContext GetSerializationContext(SerializeOptions options)
        {
            options = options ?? new SerializeOptions();
            return new ServerSerializationContext(this.typeMapper, options.ExpandedPaths ?? string.Empty, false, this.uriResolver,
                                                  this.container);
        }
    }
}