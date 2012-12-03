using System;
using System.Collections.Generic;
using System.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public class ClientTypeMapper : ITypeMapper
    {
        private readonly Dictionary<Type, SharedType> typeDict = new Dictionary<Type, SharedType>();
        private Dictionary<string, ClientType> typeNameMap;

        #region Implementation of ITypeMapper

        public IMappedType GetClassMapping(Type type)
        {
            return typeDict.GetOrCreate(
                type,
                () =>
                    {
                        SharedType sharedType;
                        if (typeof (IClientResource).IsAssignableFrom(type) && type != typeof (IClientResource))
                            sharedType = new ClientType(GetResourceNonProxyInterfaceType(type), this);
                        else
                            sharedType = new SharedType(type, this);

                        if (sharedType.MappedType == typeof (ClientRepository<,>))
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
            typeNameMap =
                clientResourceTypes.Select(GetClassMapping).Cast<ClientType>().ToDictionary(
                    x => x.ResourceInfo.JsonTypeName, x => x);
        }
    }
}