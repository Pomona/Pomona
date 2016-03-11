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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common.ExtendedResources;

namespace Pomona.Common.Proxies
{
    public class ExtendedResourceBase<TWrappedResource> : ExtendedResourceBase, IExtendedResourceProxy<TWrappedResource>
    {
    }

    public abstract class ExtendedResourceBase : IHasResourceUri, IExtendedResourceProxy
    {
        private readonly Dictionary<string, IExtendedResourceProxy> nestedProxyCache;


        protected ExtendedResourceBase()
        {
            this.nestedProxyCache = new Dictionary<string, IExtendedResourceProxy>();
        }


        internal IClientTypeResolver Client { get; private set; }


        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            if (IsServerKnownProperty(property))
                return property.Getter((TOwner)WrappedResource);

            var extProp = UserTypeInfo.ExtendedProperties.First(x => x.Property == property.PropertyInfo);
            return (TPropType)extProp.GetValue(WrappedResource, this.nestedProxyCache);

            //            // Check if this is a new'ed property on interface that is client type:
            //            ExtendedResourceInfo memberUserTypeInfo;
            //            if (ExtendedResourceInfo.TryGetExtendedResourceInfo(typeof(TPropType), out memberUserTypeInfo))
            //            {
            //                var serverProp = UserTypeInfo.ServerType.GetResourceProperty(property.Name);
            //                if (serverProp != null && serverProp.PropertyType.IsAssignableFrom(typeof (TPropType)))
            //                {
            //                    var propValue = serverProp.GetValue(this.WrappedResource, null);
            //                    if (propValue == null)
            //                        return default(TPropType);
            //#if DISABLE_PROXY_GENERATION
            //                   throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
            //#else
            //                    return (TPropType)nestedProxyCache.GetOrCreate(property.Name, () =>
            //                        {
            //                            var nestedProxy = RuntimeProxyFactory<ExtendedResourceBase, TPropType>.Create();
            //                            ((ExtendedResourceBase)((object)nestedProxy)).Initialize(Client, memberUserTypeInfo,
            //                                                                                            propValue);

            //                            return nestedProxy;
            //                        });
            //#endif
            //                }
            //            }

            //            Type elementType;
            //            if (typeof (TPropType).TryGetEnumerableElementType(out elementType) &&
            //                ExtendedResourceInfo.TryGetExtendedResourceInfo(elementType, out memberUserTypeInfo))
            //            {

            //                var serverProp = UserTypeInfo.ServerType.GetResourceProperty(property.Name);
            //                if (serverProp != null)
            //                {
            //                    var propValue = serverProp.GetValue(this.WrappedResource, null);
            //                    if (propValue == null)
            //                        return default(TPropType);

            //                    return (TPropType)nestedProxyCache.GetOrCreate(property.Name, () => createProxyListMethod.MakeGenericMethod(elementType)
            //                                         .Invoke(this, new object[] { propValue, memberUserTypeInfo }));
            //                }
            //            }

            //            if (UserTypeInfo.DictProperty == null)
            //                throw new InvalidOperationException("No attributes property to map client-side property to!");

            //            var dictValueType = UserTypeInfo.DictProperty.PropertyType.GetGenericArguments()[1];
            //            return
            //                (TPropType)
            //                onGetAttributeMethod.MakeGenericMethod(typeof(TOwner), typeof(TPropType), dictValueType)
            //                                    .Invoke(this, new object[] { property });
        }


        protected void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            object unwrappedValue = value;
            var valueAsExtendedResource = value as ExtendedResourceBase;
            if (valueAsExtendedResource != null)
                unwrappedValue = valueAsExtendedResource.WrappedResource;

            if (IsServerKnownProperty(property))
            {
                property.Set((TOwner)WrappedResource, (TPropType)unwrappedValue);
                return;
            }

            var extProp = UserTypeInfo.ExtendedProperties.First(x => x.Property == property.PropertyInfo);
            extProp.SetValue(WrappedResource, value, this.nestedProxyCache);
        }


        internal void Initialize(IClientTypeResolver client, ExtendedResourceInfo userTypeInfo, object proxyTarget)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (userTypeInfo == null)
                throw new ArgumentNullException(nameof(userTypeInfo));
            if (proxyTarget == null)
                throw new ArgumentNullException(nameof(proxyTarget));
            Client = client;
            UserTypeInfo = userTypeInfo;
            WrappedResource = proxyTarget;
        }


        private IList<TElement> CreateProxyList<TElement>(IEnumerable source, ExtendedResourceInfo userTypeInfo)
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else

            return new List<TElement>(source.Cast<object>().Select(x =>
            {
                var element = RuntimeProxyFactory<ExtendedResourceBase, TElement>.Create();
                ((ExtendedResourceBase)((object)element)).Initialize(Client, userTypeInfo, x);
                return element;
            }));
#endif
        }


        private bool IsServerKnownProperty<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return property.PropertyInfo.DeclaringType.IsInstanceOfType(WrappedResource);
        }


        public ExtendedResourceInfo UserTypeInfo { get; private set; }
        public object WrappedResource { get; private set; }

        string IHasResourceUri.Uri
        {
            get { return ((IHasResourceUri)WrappedResource).Uri; }
        }
    }
}