using System.Collections.Generic;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Json
{
    public class PomonaJsonSerializerFactory : ISerializerFactory
    {
        private Dictionary<IMappedType, PomonaJsonSerializerTypeEntry> typeCache;

        public PomonaJsonSerializerFactory() : this(Enumerable.Empty<IMappedType>())
        {
        }

        public PomonaJsonSerializerFactory(IEnumerable<IMappedType> cachedTypes)
        {
            this.typeCache = cachedTypes.ToDictionary(x => x, x => new PomonaJsonSerializerTypeEntry(x));
        }

        #region Implementation of ISerializerFactory

        public ISerializer GetSerialier()
        {
            return new PomonaJsonSerializer(this.typeCache);
        }


        public IDeserializer GetDeserializer()
        {
            return new PomonaJsonDeserializer();
        }

        #endregion
    }
}