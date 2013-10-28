#region License

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

#endregion

using System;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class ClientDeserializationContext : IDeserializationContext
    {
        private readonly IPomonaClient client;
        private readonly ITypeMapper typeMapper;

        [Obsolete("Solely here for testing purposes")]
        public ClientDeserializationContext(ITypeMapper typeMapper) : this(typeMapper, null)
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

        public void Deserialize<TReader>(IDeserializerNode node, IDeserializer<TReader> deserializer, TReader reader)
            where TReader : ISerializerReader
        {
            deserializer.DeserializeNode(node, reader);
            if (node.Uri != null)
            {
                var uriResource = node.Value as IHasResourceUri;
                if (uriResource != null)
                {
                    uriResource.Uri = node.Uri;
                }
            }
        }


        public IMappedType GetTypeByName(string typeName)
        {
            return typeMapper.GetClassMapping(typeName);
        }

        public void SetProperty(IDeserializerNode target, IPropertyInfo property, object propertyValue)
        {
            if (!property.IsWriteable)
                throw new InvalidOperationException("Unable to set property.");

            property.Setter(target.Value, propertyValue);
        }


        public void CheckPropertyAccessRights(IPropertyInfo property, PropertyAccessMode accessMode)
        {
            if (accessMode == PropertyAccessMode.ItemChangeable || accessMode == PropertyAccessMode.IsWritable)
                throw new PomonaSerializationException("Patch format not accepted from server to client.");
        }


        public IMappedType GetClassMapping(Type type)
        {
            return typeMapper.GetClassMapping(type);
        }

        public object CreateReference(IMappedType type, string uri)
        {
            if (type.SerializationMode == TypeSerializationMode.Array)
            {
                var lazyListType = typeof (LazyListProxy<>).MakeGenericType(type.ElementType.MappedTypeInstance);
                return Activator.CreateInstance(lazyListType, uri, client);
            }
            if (type is ClientType && type.SerializationMode == TypeSerializationMode.Complex)
            {
                var clientType = (ClientType) type;
                var proxyType = clientType.ResourceInfo.LazyProxyType;
                var refobj = (LazyProxyBase) Activator.CreateInstance(proxyType);
                refobj.Client = client;
                refobj.ProxyTargetType = clientType.ResourceInfo.PocoType;
                ((IHasResourceUri) refobj).Uri = uri;
                return refobj;
            }
            if (type.MappedType == typeof (ClientRepository<,>))
            {
                return Activator.CreateInstance(type.MappedTypeInstance, client, uri);
            }
            throw new NotImplementedException("Don't know how to make reference to type " + type.Name + " yet!");
        }

        #endregion
    }
}