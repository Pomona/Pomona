#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class ResourceProperty : StructuredProperty
    {
        private readonly Lazy<ResourcePropertyDetails> resourcePropertyDetails;


        public ResourceProperty(IResourceTypeResolver typeResolver,
                                PropertyInfo propertyInfo,
                                ResourceType reflectedType)
            : base(typeResolver, propertyInfo, reflectedType)
        {
            this.resourcePropertyDetails = CreateLazy(() => typeResolver.LoadResourcePropertyDetails(this));
        }


        public virtual bool ExposedAsRepository
        {
            get { return ResourcePropertyDetails.ExposedAsRepository; }
        }

        public string UriName
        {
            get { return ResourcePropertyDetails.UriName; }
        }

        protected virtual ResourcePropertyDetails ResourcePropertyDetails
        {
            get { return this.resourcePropertyDetails.Value; }
        }
    }
}