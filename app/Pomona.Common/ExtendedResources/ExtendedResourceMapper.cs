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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    public class ExtendedResourceMapper
    {
        private readonly IClientTypeResolver clientTypeResolver;

        private readonly ConcurrentDictionary<Type, ExtendedResourceInfo> extendedResourceInfoCache =
            new ConcurrentDictionary<Type, ExtendedResourceInfo>();


        public ExtendedResourceMapper(IClientTypeResolver clientTypeResolver)
        {
            if (clientTypeResolver == null)
                throw new ArgumentNullException(nameof(clientTypeResolver));
            this.clientTypeResolver = clientTypeResolver;
        }


        public object WrapForm(object serverPatchForm, Type extendedType)
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else

            var info = GetExtendedResourceInfo(extendedType);

            var userPostForm =
                (ExtendedFormBase)
                    RuntimeProxyFactory.Create(typeof(ExtendedFormBase<>).MakeGenericType(info.ServerType), info.ExtendedType);
            userPostForm.Initialize(this.clientTypeResolver, info, serverPatchForm);
            return userPostForm;
#endif
        }


        public IQueryable<T> WrapQueryable<T>(IQueryable wrappedQueryable, ExtendedResourceInfo extendedResourceInfo)
        {
            return new ExtendedQueryableRoot<T>(this.clientTypeResolver, wrappedQueryable, extendedResourceInfo, this);
        }


        public object WrapResource(object serverResource, Type serverType, Type extendedType)
        {
            return MapResult(serverResource, serverType, extendedType);
        }


        internal ExtendedResourceInfo GetExtendedResourceInfo(Type clientType)
        {
            ExtendedResourceInfo info;
            if (!TryGetExtendedResourceInfo(clientType, out info))
                throw new ArgumentException("extendedType is not inherited from a Pomona resource interface.", "extendedType");
            info.Validate();
            return info;
        }


        internal bool TryGetExtendedResourceInfo(Type clientType, out ExtendedResourceInfo info)
        {
            info = this.extendedResourceInfoCache.GetOrAdd(clientType, t => GetExtendedResourceInfoOrNull(clientType));
            return info != null;
        }


        private object CreateClientSideResourceProxy(ExtendedResourceInfo userTypeInfo,
                                                     object wrappedResource)
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else

            var proxy =
                (ExtendedResourceBase)
                    RuntimeProxyFactory.Create(typeof(ExtendedResourceBase<>).MakeGenericType(userTypeInfo.ServerType),
                                               userTypeInfo.ExtendedType);
            proxy.Initialize(this.clientTypeResolver, userTypeInfo, wrappedResource);
            return proxy;
#endif
        }


        private static PropertyInfo GetAttributesDictionaryPropertyFromResource(Type serverKnownType)
        {
            var attrProp =
                serverKnownType.GetAllInheritedPropertiesFromInterface().FirstOrDefault(
                    x => x.GetCustomAttributes(typeof(ResourceAttributesPropertyAttribute), true).Any());

            return attrProp;
        }


        private ExtendedResourceInfo GetExtendedResourceInfoOrNull(Type clientType)
        {
            ExtendedResourceInfo info = null;
            var serverTypeInfo = this.clientTypeResolver.GetMostInheritedResourceInterfaceInfo(clientType);
            if (!clientType.IsInterface || serverTypeInfo == null)
                return null;

            var serverType = serverTypeInfo.InterfaceType;

            if (serverType == clientType)
                return null;

            var dictProperty = GetAttributesDictionaryPropertyFromResource(serverType);
            info = new ExtendedResourceInfo(clientType, serverType, dictProperty, this);
            return info;
        }


        private object MapAnonymousResult(object result, Type serverType, Type extendedType)
        {
            var serverProps = serverType.GetProperties();
            var extCtor = extendedType.GetConstructors().First(x => x.GetParameters().Length == serverProps.Length);
            var args =
                (from serverProp in serverProps
                 join extProp in extendedType.GetProperties() on serverProp.Name equals extProp.Name
                 join extCtorParam in extCtor.GetParameters() on extProp.Name equals extCtorParam.Name
                 orderby extCtorParam.Position
                 select MapResult(serverProp.GetValue(result, null), serverProp.PropertyType, extProp.PropertyType)).ToArray();
            return extCtor.Invoke(args);
        }


        private object MapCollectionResult(IEnumerable<object> result, Type serverElementType, Type extendedElementType)
        {
            ExtendedResourceInfo extendedTypeInfo;
            IEnumerable<object> wrappedResults;

            if (TryGetExtendedResourceInfo(extendedElementType,
                                           out extendedTypeInfo))

            {
                if (extendedTypeInfo.ServerType != serverElementType)
                {
                    throw new ExtendedResourceMappingException(
                        "Unable to map list of extended type to correct list of server type.");
                }
                extendedTypeInfo.Validate();
                wrappedResults = result.Select(
                    x => CreateClientSideResourceProxy(extendedTypeInfo, x));
            }
            else
                wrappedResults = result.Select(x => MapResult(x, serverElementType, extendedElementType));

            // Map back to customClientType
            if (result is QueryResult)
            {
                var resultAsQueryResult = (QueryResult)result;
                return QueryResult.Create(wrappedResults,
                                          resultAsQueryResult.Skip,
                                          resultAsQueryResult.TotalCount,
                                          resultAsQueryResult.Previous,
                                          resultAsQueryResult.Next,
                                          extendedElementType);
            }
            return wrappedResults.Cast(extendedElementType).ToListDetectType();
        }


        private object MapResult(
            object result,
            Type serverType,
            Type extendedType)
        {
            if (serverType == extendedType)
                return result;

            ExtendedResourceInfo extendedTypeInfo;
            if (TryGetExtendedResourceInfo(extendedType,
                                           out extendedTypeInfo))
            {
                if (extendedTypeInfo.ServerType != serverType)
                    throw new ExtendedResourceMappingException("Unable to map extended type to correct server type.");
                extendedTypeInfo.Validate();
                return result != null
                    ? CreateClientSideResourceProxy(extendedTypeInfo, result)
                    : null;
            }

            Type extendedElementType;
            Type serverElementType;
            var isEnumerable = extendedType.TryGetEnumerableElementType(out extendedElementType)
                               & serverType.TryGetEnumerableElementType(out serverElementType);
            if (extendedType.IsAnonymous())
                return MapAnonymousResult(result, serverType, extendedType);

            if (isEnumerable)
                return MapCollectionResult(((IEnumerable)result).Cast<object>(), serverElementType, extendedElementType);
            return result;
        }
    }
}