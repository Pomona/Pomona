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
using System.Linq.Expressions;

using Pomona.Common.Internals;
using Pomona.Common.Linq;
using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    public class ExtendedQueryProvider : QueryProviderBase
    {
        private readonly IClientTypeResolver client;


        public ExtendedQueryProvider(IClientTypeResolver client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            this.client = client;
        }


        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new ExtendedQueryable<TElement>(this, expression);
        }


        public override object Execute(Expression expression)
        {
            var visitor = new TransformAdditionalPropertiesToAttributesVisitor(this.client);
            var transformedExpression = visitor.Visit(expression);
            if (visitor.Root == null)
                throw new Exception("Unable to find queryable source in expression.");
            var result = visitor.Root.WrappedSource.Provider.Execute(transformedExpression);

            return MapToCustomUserTypeResult(result, expression.Type, transformedExpression.Type);
        }


        private object CreateClientSideResourceProxy(CustomUserTypeInfo userTypeInfo,
            object wrappedResource)
        {
            var proxy =
                (ClientSideResourceProxyBase)
                    RuntimeProxyFactory.Create(typeof(ClientSideResourceProxyBase), userTypeInfo.ClientType);
            proxy.Initialize(this.client, userTypeInfo, wrappedResource);
            return proxy;
        }


        private object MapToCustomUserTypeResult(
            object result,
            Type extendedType,
            Type serverType)
        {
            CustomUserTypeInfo extendedTypeInfo;
            if (CustomUserTypeInfo.TryGetCustomUserTypeInfo(extendedType, this.client, out extendedTypeInfo))
            {
                if (extendedTypeInfo.ServerType != serverType)
                    throw new InvalidOperationException("Unable to map extended type to correct server type.");
                return result != null
                    ? CreateClientSideResourceProxy(extendedTypeInfo, result)
                    : null;
            }
            Type extendedElementType;
            if (extendedType.TryGetEnumerableElementType(out extendedElementType)
                && CustomUserTypeInfo.TryGetCustomUserTypeInfo(extendedElementType, this.client, out extendedTypeInfo))
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