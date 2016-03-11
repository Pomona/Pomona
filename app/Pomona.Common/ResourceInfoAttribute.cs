#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Pomona.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ResourceInfoAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, ResourceInfoAttribute> attributeCache =
            new ConcurrentDictionary<Type, ResourceInfoAttribute>();

        private readonly Lazy<PropertyInfo> etagProperty;
        private readonly Lazy<PropertyInfo> idProperty;


        public ResourceInfoAttribute()
        {
            this.etagProperty = new Lazy<PropertyInfo>(GetPropertyWithAttribute<ResourceEtagPropertyAttribute>);
            this.idProperty = new Lazy<PropertyInfo>(GetPropertyWithAttribute<ResourceIdPropertyAttribute>);
        }


        public Type BaseType { get; set; }

        public PropertyInfo EtagProperty
        {
            get { return this.etagProperty.Value; }
        }

        public bool HasEtagProperty
        {
            get { return this.etagProperty.Value != null; }
        }

        public bool HasIdProperty
        {
            get { return this.idProperty.Value != null; }
        }

        public PropertyInfo IdProperty
        {
            get { return this.idProperty.Value; }
        }

        public Type InterfaceType { get; set; }

        public bool IsUriBaseType
        {
            get { return UriBaseType == InterfaceType; }
        }

        public bool IsValueObject { get; set; }
        public string JsonTypeName { get; set; }
        public Type LazyProxyType { get; set; }
        public Type ParentResourceType { get; set; }
        public Type PocoType { get; set; }
        public Type PostFormType { get; set; }
        public Type UriBaseType { get; set; }
        public string UrlRelativePath { get; set; }


        public static bool TryGet(Type type, out ResourceInfoAttribute ria)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!attributeCache.TryGetValue(type, out ria))
            {
                // Only cache if not null
                var checkAttr =
                    type.GetCustomAttributes(typeof(ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>()
                        .FirstOrDefault();
                if (checkAttr == null)
                    return false;
                ria = attributeCache.GetOrAdd(type, checkAttr);
            }
            return ria != null;
        }


        private PropertyInfo GetPropertyWithAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            return InterfaceType.GetAllInheritedPropertiesFromInterface()
                                .FirstOrDefault(x => x.HasAttribute<TAttribute>(true));
        }
    }
}