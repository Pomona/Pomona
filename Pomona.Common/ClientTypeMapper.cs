using System;
using System.Collections.Generic;

using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public class ClientTypeMapper : ITypeMapper
    {
        private readonly Dictionary<Type, SharedType> typeDict = new Dictionary<Type, SharedType>();

        #region Implementation of ITypeMapper

        public IMappedType GetClassMapping(Type type)
        {
            return this.typeDict.GetOrCreate(type, () =>
            {
                SharedType sharedType = new SharedType(type, this);
                if (typeof(IClientResource).IsAssignableFrom(type))
                    sharedType.SerializationMode = TypeSerializationMode.Complex;
                return sharedType;
            });
        }

        #endregion
    }
}