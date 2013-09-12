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
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public class ClientTypeMapper : ITypeMapper
    {
        private readonly Dictionary<Type, SharedType> typeDict = new Dictionary<Type, SharedType>();
        private readonly Dictionary<string, IMappedType> typeNameMap;

        #region Implementation of ITypeMapper

        public IMappedType GetClassMapping(Type type)
        {
            return typeDict.GetOrCreate(
                type,
                () =>
                    {
                        SharedType sharedType;
                        if (typeof (IClientResource).IsAssignableFrom(type) && type != typeof (IClientResource))
                        {
                            var interfaceType = GetResourceNonProxyInterfaceType(type);
                            if (interfaceType == type)
                                sharedType = new ClientType(interfaceType, this);
                            else
                            {
                                sharedType = (ClientType) GetClassMapping(interfaceType);
                            }
                        }
                        else
                            sharedType = new SharedType(type, this);

                        if (sharedType.MappedType == typeof (ClientRepository<,>) ||
                            sharedType.MappedTypeInstance.IsAnonymous())
                            sharedType.SerializationMode = TypeSerializationMode.Complex;

                        return sharedType;
                    });
        }


        public IMappedType GetClassMapping(string typeName)
        {
            return typeNameMap[typeName];
        }

        public Type GetResourceNonProxyInterfaceType(Type type)
        {
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

        private static string GetJsonTypeName(IMappedType type)
        {
            var clientType = type as ClientType;
            if (clientType != null)
                return clientType.ResourceInfo.JsonTypeName;

            return type.Name;
        }
    }
}