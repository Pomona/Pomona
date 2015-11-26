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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Pomona.Common.ExtendedResources;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization.Json;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public class ClientTypeMapper : ExportedTypeResolverBase, IClientTypeResolver, IClientTypeFactory
    {
        private static readonly ConcurrentDictionary<Assembly, ClientTypeMapper> assemblyTypeMapperDict =
            new ConcurrentDictionary<Assembly, ClientTypeMapper>();

        private readonly ExtendedResourceMapper extendedResourceMapper;
        private readonly ReadOnlyCollection<Type> resourceTypes;
        private readonly Dictionary<string, TypeSpec> typeNameMap;


        public ClientTypeMapper(Assembly scanAssembly)
            : this(
                scanAssembly.GetTypes().Where(
                    x => IsResourceType(x) || (x.IsEnum && x.IsPublic)))
        {
        }


        public ClientTypeMapper(IEnumerable<Type> clientResourceTypes)
        {
            this.resourceTypes = clientResourceTypes.ToList().AsReadOnly();

            this.typeNameMap =
                this.resourceTypes
                    .Select(FromType)
                    .ToDictionary(GetJsonTypeName, x => x, StringComparer.OrdinalIgnoreCase);

            this.extendedResourceMapper = new ExtendedResourceMapper(this);
        }


        public IEnumerable<Type> ResourceTypes
        {
            get { return this.resourceTypes; }
        }


        public static T CreatePostForm<T>()
        {
            return PostFormFactory<T>.Create();
        }


        public override ConstructorSpec LoadConstructor(TypeSpec typeSpec)
        {
            var ria = typeSpec.DeclaredAttributes.OfType<ResourceInfoAttribute>().FirstOrDefault();
            if (ria == null)
            {
                return typeSpec.OnLoadConstructor() ??
                       ConstructorSpec.FromConstructorInfo(
                           typeSpec.Type.GetConstructors(BindingFlags.Instance | BindingFlags.Public
                                                         | BindingFlags.NonPublic).First(),
                           defaultFactory : () => null);
            }

            if (ria.PocoType == null)
                throw new NotSupportedException();

            return ConstructorSpec.FromConstructorInfo(
                ria.PocoType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic
                                             | BindingFlags.Public).First(),
                ria.InterfaceType);
        }


        public override IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec)
        {
            var typeSpec = memberSpec as RuntimeTypeSpec;
            if (typeSpec != null && typeof(IStringEnum).IsAssignableFrom(typeSpec))
            {
                return
                    base.LoadDeclaredAttributes(memberSpec).Append(
                        new CustomJsonConverterAttribute(new StringEnumJsonConverter()));
            }
            return base.LoadDeclaredAttributes(memberSpec);
        }


        public override string LoadName(MemberSpec memberSpec)
        {
            var transformedType = memberSpec as StructuredType;
            if (transformedType != null && transformedType.ResourceInfo != null)
                return transformedType.ResourceInfo.JsonTypeName;

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


        public override ResourcePropertyDetails LoadResourcePropertyDetails(ResourceProperty property)
        {
            return new ResourcePropertyDetails(false, NameUtils.ConvertCamelCaseToUri(property.Name));
        }


        public override ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType)
        {
            var ria = resourceType.DeclaredAttributes.OfType<ResourceInfoAttribute>().First();

            var pluralName = ria.UrlRelativePath != null
                ? NameUtils.ConvetUriSegmentToCamelCase(ria.UrlRelativePath)
                : null;

            return new ResourceTypeDetails(resourceType,
                                           ria.UrlRelativePath,
                                           false,
                                           resourceType,
                                           null,
                                           null,
                                           false,
                                           Enumerable.Empty<Type>(),
                                           pluralName);
        }


        public override RuntimeTypeDetails LoadRuntimeTypeDetails(TypeSpec typeSpec)
        {
            if (IsAnonType(typeSpec))
                return new RuntimeTypeDetails(TypeSerializationMode.Structured);
            return base.LoadRuntimeTypeDetails(typeSpec);
        }


        public override StructuredPropertyDetails LoadStructuredPropertyDetails(StructuredProperty property)
        {
            var propInfo = property.PropertyInfo;

            var isAttributes = propInfo.HasAttribute<ResourceAttributesPropertyAttribute>(true);
            var isPrimaryId = propInfo.HasAttribute<ResourceIdPropertyAttribute>(true);
            var isEtagProperty = propInfo.HasAttribute<ResourceEtagPropertyAttribute>(true);
            var info =
                propInfo.GetCustomAttributes(typeof(ResourcePropertyAttribute), true).OfType<ResourcePropertyAttribute>()
                        .FirstOrDefault()
                ?? new ResourcePropertyAttribute()
                {
                    AccessMode = HttpMethod.Get,
                    ItemAccessMode = HttpMethod.Get,
                    Required = false
                };

            return new StructuredPropertyDetails(isAttributes,
                                                 isEtagProperty,
                                                 isPrimaryId,
                                                 true,
                                                 info.AccessMode,
                                                 info.ItemAccessMode,
                                                 ExpandMode.Full);
        }


        public override StructuredTypeDetails LoadStructuredTypeDetails(StructuredType structuredType)
        {
            if (IsAnonType(structuredType) || structuredType is QueryResultType)
            {
                return new StructuredTypeDetails(structuredType,
                                                 HttpMethod.Get,
                                                 null,
                                                 true,
                                                 true,
                                                 false);
            }

            var ria = structuredType.DeclaredAttributes.OfType<ResourceInfoAttribute>().First();
            var allMethods = (HttpMethod.Delete | HttpMethod.Get | HttpMethod.Patch | HttpMethod.Post | HttpMethod.Put);
            var allowedMethods =
                structuredType
                    .DeclaredAttributes
                    .OfType<AllowedMethodsAttribute>()
                    .Select(x => (HttpMethod?)x.Methods)
                    .FirstOrDefault() ?? allMethods;

            return new StructuredTypeDetails(structuredType,
                                             allowedMethods,
                                             null,
                                             ria.IsValueObject,
                                             true,
                                             false);
        }


        public override IEnumerable<StructuredType> LoadSubTypes(StructuredType baseType)
        {
            return this.typeNameMap.Values.OfType<StructuredType>();
        }


        public bool TryGetExtendedTypeInfo(Type type, out ExtendedResourceInfo userTypeInfo)
        {
            return this.extendedResourceMapper.TryGetExtendedResourceInfo(type, out userTypeInfo);
        }


        public override bool TryGetTypeByName(string typeName, out TypeSpec typeSpec)
        {
            return base.TryGetTypeByName(typeName, out typeSpec) || this.typeNameMap.TryGetValue(typeName, out typeSpec);
        }


        public IQueryable<T> WrapExtendedQuery<T>(Func<Type, IQueryable> queryableCreator)
        {
            ExtendedResourceInfo extendedResourceInfo;
            if (TryGetExtendedTypeInfo(typeof(T), out extendedResourceInfo))
            {
                var wrappedQueryable = queryableCreator(extendedResourceInfo.ServerType);

                return this.extendedResourceMapper.WrapQueryable<T>(wrappedQueryable, extendedResourceInfo);
            }
            return (IQueryable<T>)queryableCreator(typeof(T));
        }


        public object WrapResource(object serverResource, Type serverType, Type extendedType)
        {
            return this.extendedResourceMapper.WrapResource(serverResource, serverType, extendedType);
        }


        protected override TypeSpec CreateType(Type type)
        {
            var ria =
                type.GetCustomAttributes(typeof(ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>()
                    .FirstOrDefault();
            if (ria != null && typeof(IClientResource).IsAssignableFrom(type))
            {
                if (ria.IsValueObject)
                    return new ComplexType(this, type);
                return new ResourceType(this, type);
            }

            if (IsAnonType(type))
                return new AnonymousType(this, type);
            return base.CreateType(type);
        }


        protected override sealed Type MapExposedClrType(Type type)
        {
            Type[] proxyTypeArgs;
            if (typeof(IExtendedResourceProxy).IsAssignableFrom(type)
                && type.TryExtractTypeArguments(typeof(IExtendedResourceProxy<>), out proxyTypeArgs))
                type = proxyTypeArgs[0];

            if (!type.IsInterface)
            {
                if (typeof(IClientResource).IsAssignableFrom(type))
                    return GetMainInterfaceFromConcreteType(typeof(IClientResource), type);
                if (typeof(IClientRepository).IsAssignableFrom(type))
                    return GetMainInterfaceFromConcreteType(typeof(IClientRepository), type);
            }

            return type;
        }


        internal static ClientTypeMapper GetTypeMapper(Type type)
        {
            var assembly = type.Assembly;
            return assemblyTypeMapperDict.GetOrAdd(assembly, x => new ClientTypeMapper(x));
        }


        private static string GetJsonTypeName(TypeSpec type)
        {
            var clientType = type as StructuredType;

            if (clientType != null)
                return clientType.ResourceInfo.JsonTypeName;

            return type.Name;
        }


        private static Type GetMainInterfaceFromConcreteType(Type interfaceType, Type type)
        {
            var interfaces =
                type.GetInterfaces().Where(interfaceType.IsAssignableFrom).ToArray();
            IEnumerable<Type> exceptTheseInterfaces =
                interfaces.SelectMany(
                    x => x.GetInterfaces().Where(interfaceType.IsAssignableFrom)).
                           Distinct().ToArray();

            var mostSubtypedInterface =
                interfaces
                    .Except(exceptTheseInterfaces).Single(x => !x.IsGenericType);

            type = mostSubtypedInterface;
            return type;
        }


        private static bool IsAnonType(Type type)
        {
            return type.IsAnonymous() || type.IsTuple();
        }


        private static bool IsResourceType(Type x)
        {
            return x.IsInterface && typeof(IClientResource).IsAssignableFrom(x)
                   && x.HasAttribute<ResourceInfoAttribute>(false);
        }


        public object CreatePatchForm(Type resourceType, object original)
        {
            var extendedResourceProxy = original as ExtendedResourceBase;

            if (extendedResourceProxy != null)
            {
                var info = extendedResourceProxy.UserTypeInfo;
                return
                    this.extendedResourceMapper.WrapForm(
                        CreatePatchForm(info.ServerType, extendedResourceProxy.WrappedResource),
                        info.ExtendedType);
            }

            var resourceInfo = this.GetResourceInfoForType(resourceType);
            if (!resourceType.GetCustomAttributes(typeof(AllowedMethodsAttribute), false)
                             .OfType<AllowedMethodsAttribute>()
                             .Select(x => x.Methods)
                             .FirstOrDefault()
                             .HasFlag(HttpMethod.Patch))
                throw new InvalidOperationException("Method PATCH is not allowed for uri.");

            var serverPatchForm = ObjectDeltaProxyBase.CreateDeltaProxy(original,
                                                                        FromType(
                                                                            resourceInfo.InterfaceType),
                                                                        this,
                                                                        null,
                                                                        resourceInfo.InterfaceType);

            return serverPatchForm;
        }


        public IPostForm CreatePostForm(Type resourceType)
        {
            ExtendedResourceInfo extendedResourceInfo;

            if (TryGetExtendedTypeInfo(resourceType, out extendedResourceInfo))
            {
                return (IPostForm)this.extendedResourceMapper.WrapForm(CreatePostForm(extendedResourceInfo.ServerType),
                                                            extendedResourceInfo.ExtendedType);
            }

            var resourceInfo = this.GetResourceInfoForType(resourceType);
            if (resourceInfo.PostFormType == null)
                throw new InvalidOperationException("Method POST is not allowed for uri.");
            var serverPostForm = Activator.CreateInstance(resourceInfo.PostFormType);
            return (IPostForm)serverPostForm;
        }


        public bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return ResourceInfoAttribute.TryGet(type, out resourceInfo);
        }


        private static class PostFormFactory<T>
        {
            private static Func<T> factory;


            public static T Create()
            {
                if (factory == null)
                {
                    var type = typeof(T);
                    var realInterface = ClientTypeResolver.Default.GetMostInheritedResourceInterface(type);
                    var clientTypeMapper = GetTypeMapper(realInterface);
                    factory = () => (T)clientTypeMapper.CreatePostForm(type);
                }
                return factory();
            }
        }
    }
}