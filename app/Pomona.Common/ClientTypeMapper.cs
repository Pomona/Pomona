// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public class ClientTypeMapper : ExportedTypeResolverBase, ITypeMapper
    {
        private readonly Dictionary<string, TypeSpec> typeNameMap;

        #region Implementation of ITypeMapper

        public TypeSpec GetClassMapping(Type type)
        {
            return FromType(GetResourceNonProxyInterfaceType(type));
        }

        public TypeSpec GetClassMapping(string typeName)
        {
            return typeNameMap[typeName];
        }

        public Type GetResourceNonProxyInterfaceType(Type type)
        {
            if (!typeof(IClientResource).IsAssignableFrom(type))
                return type;

            if (!type.IsInterface)
            {
                var interfaces =
                    type.GetInterfaces().Where(x => typeof (IClientResource).IsAssignableFrom(x)).ToArray();
                IEnumerable<Type> exceptTheseInterfaces =
                    interfaces.SelectMany(
                        x => x.GetInterfaces().Where(y => typeof (IClientResource).IsAssignableFrom(y))).
                               Distinct().ToArray();
                var mostSubtypedInterface =
                    interfaces
                        .Except(
                            exceptTheseInterfaces)
                        .Single();

                type = mostSubtypedInterface;
            }

            return type;
        }

        #endregion

        public ClientTypeMapper(IEnumerable<Type> clientResourceTypes)
        {
            var mappedTypes = clientResourceTypes.Union(TypeUtils.GetNativeTypes());
            typeNameMap =
                mappedTypes
                    .Select(GetClassMapping)
                    .ToDictionary(GetJsonTypeName, x => x);
        }

        private static string GetJsonTypeName(TypeSpec type)
        {
            var clientType = type as TransformedType;

            if (clientType != null)
                return clientType.ResourceInfo.JsonTypeName;

            return type.Name;
        }


        public override string LoadName(MemberSpec memberSpec)
        {
            var transformedType = memberSpec as TransformedType;
            if (transformedType != null && transformedType.ResourceInfo != null)
            {
                return transformedType.ResourceInfo.JsonTypeName;
            }

            return base.LoadName(memberSpec);
        }

        public override IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec)
        {
            var ria =
                typeSpec.Type.GetCustomAttributes(typeof(ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>()
                    .FirstOrDefault();
            if (ria == null)
                return base.LoadProperties(typeSpec);
             return
                typeSpec.Type.GetProperties()
                    .Concat(typeSpec.Type.GetInterfaces().SelectMany(x => x.GetProperties()))
                    .Select(x => WrapProperty(typeSpec, x));
        }

        public override IEnumerable<TransformedType> GetAllTransformedTypes()
        {
            return typeNameMap.Values.OfType<TransformedType>();
        }


        public override ExportedPropertyDetails LoadExportedPropertyDetails(PropertyMapping propertyMapping)
        {
            var propInfo = propertyMapping.PropertyInfo;

            var isAttributes = propInfo.HasAttribute<ResourceAttributesPropertyAttribute>(true);
            var isPrimaryId = propInfo.HasAttribute<ResourceIdPropertyAttribute>(true);
            var isEtagProperty = propInfo.HasAttribute<ResourceEtagPropertyAttribute>(true);
            var info = propInfo.GetCustomAttributes(typeof(ResourcePropertyAttribute), true).OfType<ResourcePropertyAttribute>().FirstOrDefault()
                       ?? new ResourcePropertyAttribute()
            {
                AccessMode = HttpMethod.Get,
                ItemAccessMode = HttpMethod.Get,
                Required = false
            };

            return new ExportedPropertyDetails(isAttributes,
                isEtagProperty,
                isPrimaryId,
                info.AccessMode,
                info.ItemAccessMode,
                false,
                NameUtils.ConvertCamelCaseToUri(propertyMapping.Name),
                true /* ??AlwaysExpand should be true on client, right? */);
        }


        protected override TypeSpec CreateType(Type type)
        {
            var ria =
                type.GetCustomAttributes(typeof(ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>()
                    .FirstOrDefault();
            if (ria != null && typeof(IClientResource).IsAssignableFrom(type))
            {
                if (ria.IsValueObject)
                    return new TransformedType(this, type);
                return new ResourceType(this, type);
            }
            if (type.IsAnonymous())
                return new TransformedType(this, type);
            return base.CreateType(type);
        }


        public override ConstructorSpec LoadConstructor(TypeSpec typeSpec)
        {
            var ria = typeSpec.DeclaredAttributes.OfType<ResourceInfoAttribute>().FirstOrDefault();
            if (ria == null)
                return
                    ConstructorSpec.FromConstructorInfo(
                        typeSpec.Type.GetConstructors(BindingFlags.Instance | BindingFlags.Public
                                                      | BindingFlags.NonPublic).First(), defaultFactory: () => null);

            if (ria.PocoType == null)
                throw new NotSupportedException();

            return ConstructorSpec.FromConstructorInfo(
                ria.PocoType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic
                                              | BindingFlags.Public).First(), ria.InterfaceType);
        }


        [PendingReview]
        public override ExportedTypeDetails LoadExportedTypeDetails(TransformedType exportedType)
        {
            if (exportedType.IsAnonymous())
                return new ExportedTypeDetails(exportedType,
                    HttpMethod.Get,
                    null,
                    null,
                    true,
                    true);

            var ria = exportedType.DeclaredAttributes.OfType<ResourceInfoAttribute>().First();
            var allowedMethods = (ria.PostFormType != null ? HttpMethod.Post : 0)
                                 | (ria.PatchFormType != null ? HttpMethod.Patch : 0) | HttpMethod.Get;
            return new ExportedTypeDetails(exportedType,
                allowedMethods,
                "??whatever??TODO",
                null,   
                ria.IsValueObject,
                true);
        }


        public override ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType)
        {
            var ria = resourceType.DeclaredAttributes.OfType<ResourceInfoAttribute>().First();

            return new ResourceTypeDetails(resourceType, ria.UrlRelativePath, false, resourceType, null, null);
        }
    }
}