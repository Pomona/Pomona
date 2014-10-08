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
using System.Collections.Concurrent;
using System.Linq;

using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientDeserializationContext : IDeserializationContext
    {
        private readonly IPomonaClient client;
        private readonly ITypeMapper typeMapper;


        [Obsolete("Solely here for testing purposes")]
        public ClientDeserializationContext(ITypeMapper typeMapper)
            : this(typeMapper, null)
        {
        }


        public ClientDeserializationContext(ITypeMapper typeMapper, IPomonaClient client)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
            this.client = client;
        }

        #region Implementation of IDeserializationContext

        private readonly ConcurrentDictionary<Type, Type> clientRepositoryImplementationMap =
            new ConcurrentDictionary<Type, Type>();

        public IResourceNode TargetNode
        {
            get { return null; }
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
            if (type is TransformedType && type.SerializationMode == TypeSerializationMode.Complex)
            {
                var clientType = (TransformedType)type;
                var proxyType = clientType.ResourceInfo.LazyProxyType;
                var refobj = (LazyProxyBase)Activator.CreateInstance(proxyType);
                refobj.Client = this.client;
                refobj.ProxyTargetType = clientType.ResourceInfo.PocoType;
                ((IHasResourceUri)refobj).Uri = uri;
                return refobj;
            }
            throw new NotImplementedException("Don't know how to make reference to type " + type.Name + " yet!");
        }


        public object CreateResource<T>(TransformedType type, IConstructorPropertySource<T> args)
        {
            return type.Create(args);
        }


        public void Deserialize(IDeserializerNode node, Action<IDeserializerNode> deserializeNodeAction)
        {
            deserializeNodeAction(node);
            if (node.Uri != null)
            {
                var uriResource = node.Value as IHasResourceUri;
                if (uriResource != null)
                    uriResource.Uri = node.Uri;
            }
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return this.typeMapper.GetClassMapping(type);
        }


        public T GetInstance<T>()
        {
            throw new NotSupportedException();
        }


        public TypeSpec GetTypeByName(string typeName)
        {
            return this.typeMapper.GetClassMapping(typeName);
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
                    t => t.Assembly.GetTypes().First(x => !x.IsInterface && x.IsClass && t.IsAssignableFrom(x)));

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

        #endregion
    }
}