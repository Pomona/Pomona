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
using System.Collections;
using System.Linq;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    public class ExtendedResourceMapper
    {
        private readonly IClientTypeResolver clientTypeResolver;


        public ExtendedResourceMapper(IClientTypeResolver clientTypeResolver)
        {
            this.clientTypeResolver = clientTypeResolver;
        }


        public IQueryable<T> WrapQueryable<T>(IQueryable wrappedQueryable, ExtendedResourceInfo extendedResourceInfo)
        {
            return new ExtendedQueryableRoot<T>(clientTypeResolver, wrappedQueryable, extendedResourceInfo);
        }

        public object WrapForm(object serverPatchForm, Type extendedType)
        {
            ExtendedResourceInfo info;
            if (!ExtendedResourceInfo.TryGetExtendedResourceInfo(extendedType, clientTypeResolver, out info))
                throw new ArgumentException("extendedType is not inherited from a Pomona resource interface.", "extendedType");

            var userPostForm =
                (ExtendedFormBase)
                    RuntimeProxyFactory.Create(typeof(ExtendedFormBase), info.ExtendedType);
            userPostForm.Initialize(this.clientTypeResolver, info, serverPatchForm);
            return userPostForm;
        }


        public object WrapResource(object serverResource, Type serverType, Type extendedType)
        {
            return MapToCustomUserTypeResult(serverResource, serverType, extendedType);
        }


        private object CreateClientSideResourceProxy(ExtendedResourceInfo userTypeInfo,
            object wrappedResource)
        {
            var proxy =
                (ExtendedResourceBase)
                    RuntimeProxyFactory.Create(typeof(ExtendedResourceBase), userTypeInfo.ExtendedType);
            proxy.Initialize(this.clientTypeResolver, userTypeInfo, wrappedResource);
            return proxy;
        }


        private object MapToCustomUserTypeResult(
            object result,
            Type serverType,
            Type extendedType)
        {
            ExtendedResourceInfo extendedTypeInfo;
            if (ExtendedResourceInfo.TryGetExtendedResourceInfo(extendedType,
                this.clientTypeResolver,
                out extendedTypeInfo))
            {
                if (extendedTypeInfo.ServerType != serverType)
                    throw new InvalidOperationException("Unable to map extended type to correct server type.");
                return result != null
                    ? CreateClientSideResourceProxy(extendedTypeInfo, result)
                    : null;
            }
            Type extendedElementType;
            if (extendedType.TryGetEnumerableElementType(out extendedElementType)
                && ExtendedResourceInfo.TryGetExtendedResourceInfo(extendedElementType,
                    this.clientTypeResolver,
                    out extendedTypeInfo))
            {
                Type serverElementType;
                if (!serverType.TryGetEnumerableElementType(out serverElementType)
                    || extendedTypeInfo.ServerType != serverElementType)
                {
                    throw new InvalidOperationException(
                        "Unable to map list of extended type to correct list of server type.");
                }
                var wrappedResults =
                    ((IEnumerable)result).Cast<object>()
                        .Select(
                            x => CreateClientSideResourceProxy(extendedTypeInfo, x));
                // Map back to customClientType
                if (result is QueryResult)
                {
                    var resultAsQueryResult = (QueryResult)result;
                    return QueryResult.Create(wrappedResults,
                        resultAsQueryResult.Skip,
                        resultAsQueryResult.TotalCount,
                        resultAsQueryResult.Url,
                        extendedElementType);
                }
                return wrappedResults.ToList();
            }
            return result;
        }
    }
}