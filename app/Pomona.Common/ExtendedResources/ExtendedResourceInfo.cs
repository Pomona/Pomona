#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
        private readonly Type serverType;


        private ExtendedResourceInfo(Type extendedType, Type serverType, PropertyInfo dictProperty)
        {
            this.extendedType = extendedType;
            this.serverType = serverType;
            this.dictProperty = dictProperty;
            Type[] dictTypeArgs;
            if (dictProperty != null
                && dictProperty.PropertyType.TryExtractTypeArguments(typeof(IDictionary<,>), out dictTypeArgs))
                dictValueType = dictTypeArgs[1];
            this.extendedProperties =
                new Lazy<ReadOnlyCollection<ExtendedProperty>>(() => InitializeExtendedProperties().ToList().AsReadOnly());
        }


        public Type ExtendedType
        {
            get { return this.extendedType; }
        }

        public Type ServerType
        {
            get { return this.serverType; }
        }

        internal ReadOnlyCollection<ExtendedProperty> ExtendedProperties
        {
            get { return this.extendedProperties.Value; }
        }

        internal Type DictValueType
        {
            get { return dictValueType; }
        }

        public PropertyInfo DictProperty
        {
            get { return this.dictProperty; }
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
            var serverProp = serverType.GetPropertySearchInheritedInterfaces(extendedProp.Name);
            var extPropType = extendedProp.PropertyType;
            if (serverProp != null)
            {
                var serverPropType = serverProp.PropertyType;
                ExtendedResourceInfo propExtInfo;
                if (TryGetExtendedResourceInfo(extPropType, out propExtInfo)
                    && typeof(IClientResource).IsAssignableFrom(serverPropType))
                    return new ExtendedComplexOverlayProperty(extendedProp, serverProp, propExtInfo);
                Type extPropElementType;

                Type serverPropElementType;
                if (extPropType.TryGetEnumerableElementType(out extPropElementType)
                    && TryGetExtendedResourceInfo(extPropElementType, out propExtInfo)
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
                    throw new ExtendedResourceMappingException(string.Format(
                        "Unable to map property {0} of type {1} to underlying dictionary property {2} of {3}. Only nullable value types can be mapped to a dictionary.",
                        extendedProp.Name, extendedType.FullName, dictProperty.Name, serverType.FullName));
                }
            }
            throw new ExtendedResourceMappingException(
                string.Format(
                    "Unable to map property {0} of type {1} to any underlying dictionary property having a [ResourceAttributesProperty] on {2}.",
                    extendedProp.Name, extendedType.FullName, serverType.FullName));
        }


        internal static bool TryGetExtendedResourceInfo(Type clientType, out ExtendedResourceInfo info)
        {
            info = null;
            var serverTypeInfo = ClientTypeResolver.Default.GetMostInheritedResourceInterfaceInfo(clientType);
            if (!clientType.IsInterface || serverTypeInfo == null)
                return false;

            var serverType = serverTypeInfo.InterfaceType;

            if (serverType == clientType)
                return false;

            var dictProperty = GetAttributesDictionaryPropertyFromResource(serverType);
            info = new ExtendedResourceInfo(clientType, serverType, dictProperty);
            return true;
        }


        private static PropertyInfo GetAttributesDictionaryPropertyFromResource(Type serverKnownType)
        {
            var attrProp =
                serverKnownType.GetAllInheritedPropertiesFromInterface().FirstOrDefault(
                    x => x.GetCustomAttributes(typeof(ResourceAttributesPropertyAttribute), true).Any());

            return attrProp;
        }
    }
}