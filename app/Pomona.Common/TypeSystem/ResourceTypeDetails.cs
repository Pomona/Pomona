#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class ResourceTypeDetails
    {
        private readonly PropertyInfo childToParentPropertyInfo;
        private readonly PropertyInfo parentToChildPropertyInfo;
        private readonly Type postReturnType;
        private readonly ResourceType type;


        public ResourceTypeDetails(ResourceType type,
                                   string urlRelativePath,
                                   bool isExposedAsRepository,
                                   Type postReturnType,
                                   PropertyInfo parentToChildPropertyInfo,
                                   PropertyInfo childToParentPropertyInfo,
                                   bool isSingleton,
                                   IEnumerable<Type> resourceHandlers,
                                   string pluralName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            this.type = type;
            UrlRelativePath = urlRelativePath;
            IsExposedAsRepository = isExposedAsRepository;
            this.postReturnType = postReturnType;
            this.parentToChildPropertyInfo = parentToChildPropertyInfo;
            this.childToParentPropertyInfo = childToParentPropertyInfo;
            IsSingleton = isSingleton;
            ResourceHandlers = resourceHandlers;
            PluralName = pluralName;
        }


        public ResourceProperty ChildToParentProperty => (ResourceProperty)this.type.GetPropertyByName(this.childToParentPropertyInfo.Name, false);

        public ResourceProperty ETagProperty
        {
            get { return this.type.Properties.FirstOrDefault(x => x.IsEtagProperty); }
        }

        public bool IsExposedAsRepository { get; }

        public bool IsSingleton { get; }

        public ResourceType ParentResourceType => ParentToChildProperty != null ? (ResourceType)ParentToChildProperty.DeclaringType : null;

        public ResourceProperty ParentToChildProperty => this.parentToChildPropertyInfo != null
            ? (ResourceProperty)
                this.type.TypeResolver.FromType(this.parentToChildPropertyInfo.DeclaringType).GetPropertyByName(
                    this.parentToChildPropertyInfo.Name,
                    false)
            : null;

        public string PluralName { get; }

        public StructuredType PostReturnType => (StructuredType)this.type.TypeResolver.FromType(this.postReturnType);

        public IEnumerable<Type> ResourceHandlers { get; }

        public string UrlRelativePath { get; }
    }
}