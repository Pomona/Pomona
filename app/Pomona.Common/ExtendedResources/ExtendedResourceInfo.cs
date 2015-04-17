#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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
        private readonly PropertyInfo dictProperty;
        private readonly Type dictValueType;
        private readonly Lazy<ReadOnlyCollection<ExtendedProperty>> extendedProperties;
        private readonly Type extendedType;
        private readonly ExtendedResourceMapper mapper;
        private readonly Type serverType;


        internal ExtendedResourceInfo(Type extendedType, Type serverType, PropertyInfo dictProperty, ExtendedResourceMapper mapper)
        {
            this.extendedType = extendedType;
            this.serverType = serverType;
            this.dictProperty = dictProperty;
            this.mapper = mapper;
            Type[] dictTypeArgs;
            if (dictProperty != null
                && dictProperty.PropertyType.TryExtractTypeArguments(typeof(IDictionary<,>), out dictTypeArgs))
                this.dictValueType = dictTypeArgs[1];
            this.extendedProperties =
                new Lazy<ReadOnlyCollection<ExtendedProperty>>(() => InitializeExtendedProperties().ToList().AsReadOnly());
        }


        public PropertyInfo DictProperty
        {
            get { return this.dictProperty; }
        }

        public Type ExtendedType
        {
            get { return this.extendedType; }
        }

        public Type ServerType
        {
            get { return this.serverType; }
        }

        internal Type DictValueType
        {
            get { return this.dictValueType; }
        }

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
            return this.extendedType
                       .WrapAsEnumerable()
                       .Concat(this.extendedType.GetInterfaces().Where(x => !x.IsAssignableFrom(this.serverType)))
                       .SelectMany(x => x.GetProperties())
                       .Distinct();
        }


        private IEnumerable<ExtendedProperty> InitializeExtendedProperties()
        {
            return GetAllExtendedPropertiesFromType().Select(InitializeProperty);
        }


        private ExtendedProperty InitializeProperty(PropertyInfo extendedProp)
        {
            var serverProp = this.serverType.GetPropertySearchInheritedInterfaces(extendedProp.Name);
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
            else if (this.dictProperty != null)
            {
                if (!extPropType.IsValueType || extPropType.IsNullable())
                    return ExtendedAttributeProperty.Create(extendedProp, this);
                else
                {
                    var message = string.Format(
                        "Unable to map property {0} of type {1} to underlying dictionary property {2} of {3}. Only nullable value types can be mapped to a dictionary.",
                        extendedProp.Name, this.extendedType.FullName, this.dictProperty.Name, this.serverType.FullName);
                    return new InvalidExtendedProperty(extendedProp, message);
                }
            }
            return new InvalidExtendedProperty(extendedProp, string.Format(
                "Unable to map property {0} of type {1} to any underlying dictionary property having a [ResourceAttributesProperty] on {2}.",
                extendedProp.Name, this.extendedType.FullName, this.serverType.FullName));
        }
    }
}