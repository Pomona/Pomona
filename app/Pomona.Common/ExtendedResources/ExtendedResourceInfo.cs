#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.ExtendedResources
{
    public class ExtendedResourceInfo
    {
        private readonly Lazy<ReadOnlyCollection<ExtendedProperty>> extendedProperties;
        private readonly ExtendedResourceMapper mapper;


        internal ExtendedResourceInfo(Type extendedType, Type serverType, PropertyInfo dictProperty, ExtendedResourceMapper mapper)
        {
            ExtendedType = extendedType;
            ServerType = serverType;
            DictProperty = dictProperty;
            this.mapper = mapper;
            Type[] dictTypeArgs;
            if (dictProperty != null
                && dictProperty.PropertyType.TryExtractTypeArguments(typeof(IDictionary<,>), out dictTypeArgs))
                DictValueType = dictTypeArgs[1];
            this.extendedProperties =
                new Lazy<ReadOnlyCollection<ExtendedProperty>>(() => InitializeExtendedProperties().ToList().AsReadOnly());
        }


        public PropertyInfo DictProperty { get; }

        public Type ExtendedType { get; }

        public Type ServerType { get; }

        internal Type DictValueType { get; }

        internal ReadOnlyCollection<ExtendedProperty> ExtendedProperties
        {
            get { return this.extendedProperties.Value; }
        }


        internal void Validate()
        {
            foreach (var prop in ExtendedProperties.OfType<InvalidExtendedProperty>())
                throw new ExtendedResourceMappingException(prop.ErrorMessage);
        }


        private IEnumerable<PropertyInfo> GetAllExtendedPropertiesFromType()
        {
            return ExtendedType
                .WrapAsEnumerable()
                .Concat(ExtendedType.GetInterfaces().Where(x => !x.IsAssignableFrom(ServerType)))
                .SelectMany(x => x.GetProperties())
                .Distinct();
        }


        private IEnumerable<ExtendedProperty> InitializeExtendedProperties()
        {
            return GetAllExtendedPropertiesFromType().Select(InitializeProperty);
        }


        private ExtendedProperty InitializeProperty(PropertyInfo extendedProp)
        {
            var serverProp = ServerType.GetPropertySearchInheritedInterfaces(extendedProp.Name);
            var extPropType = extendedProp.PropertyType;
            if (serverProp != null)
            {
                var serverPropType = serverProp.PropertyType;
                ExtendedResourceInfo propExtInfo;
                if (this.mapper.TryGetExtendedResourceInfo(extPropType, out propExtInfo)
                    && typeof(IClientResource).IsAssignableFrom(serverPropType))
                    return new ExtendedComplexOverlayProperty(extendedProp, serverProp, propExtInfo);
                Type extPropElementType;

                Type serverPropElementType;
                if (extPropType.TryGetEnumerableElementType(out extPropElementType)
                    && this.mapper.TryGetExtendedResourceInfo(extPropElementType, out propExtInfo)
                    && serverPropType.TryGetEnumerableElementType(out serverPropElementType)
                    && serverPropElementType == propExtInfo.ServerType)
                    return new ExtendedCollectionOverlayProperty(extendedProp, serverProp, propExtInfo);
            }
            else if (DictProperty != null)
            {
                if (!extPropType.IsValueType || extPropType.IsNullable())
                    return ExtendedAttributeProperty.Create(extendedProp, this);
                else
                {
                    var message = string.Format(
                        "Unable to map property {0} of type {1} to underlying dictionary property {2} of {3}. Only nullable value types can be mapped to a dictionary.",
                        extendedProp.Name, ExtendedType.FullName, DictProperty.Name, ServerType.FullName);
                    return new InvalidExtendedProperty(extendedProp, message);
                }
            }
            return new InvalidExtendedProperty(extendedProp, string.Format(
                "Unable to map property {0} of type {1} to any underlying dictionary property having a [ResourceAttributesProperty] on {2}.",
                extendedProp.Name, ExtendedType.FullName, ServerType.FullName));
        }
    }
}