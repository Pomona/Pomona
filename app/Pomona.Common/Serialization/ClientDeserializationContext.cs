#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;

using Pomona.Common.Loading;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientDeserializationContext : IDeserializationContext
    {
        private readonly IPomonaClient client;

        private readonly ConcurrentDictionary<Type, Type> clientRepositoryImplementationMap =
            new ConcurrentDictionary<Type, Type>();

        private readonly IResourceLoader resourceLoader;
        private readonly ITypeResolver typeMapper;


        public ClientDeserializationContext(ITypeResolver typeMapper,
                                            IPomonaClient client,
                                            IResourceLoader resourceLoader)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));

            if (resourceLoader == null)
                throw new ArgumentNullException(nameof(resourceLoader));

            this.typeMapper = typeMapper;
            this.client = client;
            this.resourceLoader = resourceLoader;
        }


        [Obsolete("Solely here for testing purposes")]
        internal ClientDeserializationContext(ITypeResolver typeMapper, IResourceLoader resourceLoader)
            : this(typeMapper, null, resourceLoader)
        {
        }


        public void CheckAccessRights(PropertySpec property, HttpMethod method)
        {
        }


        public void CheckPropertyItemAccessRights(PropertySpec property, HttpMethod method)
        {
            if (method == HttpMethod.Patch || method == HttpMethod.Put || method == HttpMethod.Delete)
                throw new PomonaSerializationException("Patch format not accepted from server to client.");
        }


        public object CreateReference(IDeserializerNode node)
        {
            var uri = node.Uri;
            var type = node.ValueType;
            /*
            if (type.Type.IsGenericType && type.Type.GetGenericTypeDefinition() == typeof(ClientRepository<,>))
            {
                if (node.Parent != null)
                {
                    var childRepositoryType =
                        typeof(ChildResourceRepository<,>).MakeGenericType(type.Type.GetGenericArguments());
                    return Activator.CreateInstance(childRepositoryType, client, uri, null, node.Parent.Value);
                }
                return Activator.CreateInstance(type.Type, client, uri, null);
            }*/

            if (type.SerializationMode == TypeSerializationMode.Array)
                return LazyCollectionProxy.CreateForType(type, uri, this.client);

            if (type is StructuredType && type.SerializationMode == TypeSerializationMode.Structured)
            {
                var clientType = (StructuredType)type;
                var proxyType = clientType.ResourceInfo.LazyProxyType;
                var refobj = (LazyProxyBase)Activator.CreateInstance(proxyType);
                refobj.Initialize(uri, this.resourceLoader, clientType.ResourceInfo.PocoType, node.ExpandPath);
                return refobj;
            }

            throw new NotImplementedException("Don't know how to make reference to type " + type.Name + " yet!");
        }


        public object CreateResource(TypeSpec type, IConstructorPropertySource args)
        {
            return type.Create(args);
        }


        public void Deserialize(IDeserializerNode node, Action<IDeserializerNode> deserializeNodeAction)
        {
            deserializeNodeAction(node);
            if (node.Uri != null)
            {
                var uriResource = node.Value as IHasSettableResourceUri;
                if (uriResource != null)
                    uriResource.Uri = node.Uri;
            }
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return this.typeMapper.FromType(type);
        }


        public T GetInstance<T>()
        {
            throw new NotSupportedException();
        }


        public TypeSpec GetTypeByName(string typeName)
        {
            return this.typeMapper.FromType(typeName);
        }


        public void OnMissingRequiredPropertyError(IDeserializerNode node, PropertySpec targetProp)
        {
        }


        public void SetProperty(IDeserializerNode target, PropertySpec property, object propertyValue)
        {
            if (!property.IsWritable)
                throw new InvalidOperationException("Unable to set property.");

            if (typeof(IClientRepository).IsAssignableFrom(property.PropertyType))
            {
                var repoImplementationType = this.clientRepositoryImplementationMap.GetOrAdd(property.PropertyType,
                                                                                             t =>
                                                                                                 t.Assembly.GetTypes()
                                                                                                  .First(
                                                                                                      x =>
                                                                                                          !x.IsInterface
                                                                                                          && x.IsClass
                                                                                                          && t
                                                                                                          .IsAssignableFrom
                                                                                                          (x)));

                var listProxyValue = propertyValue as LazyCollectionProxy;
                object repo;
                if (listProxyValue != null)
                {
                    repo = Activator.CreateInstance(repoImplementationType,
                                                    this.client,
                                                    listProxyValue.Uri,
                                                    null,
                                                    target.Value);
                }
                else
                {
                    repo = Activator.CreateInstance(repoImplementationType,
                                                    this.client,
                                                    target.Uri + "/" + NameUtils.ConvertCamelCaseToUri(property.Name),
                                                    propertyValue,
                                                    target.Value);
                }
                property.SetValue(target.Value, repo);
                return;
            }

            property.SetValue(target.Value, propertyValue);
        }


        public IResourceNode TargetNode => null;
    }
}