using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public class ClientType : SharedType
    {
        private readonly ResourceInfoAttribute resourceInfo;

        public ClientType(Type mappedTypeInstance, ITypeMapper typeMapper) : base(mappedTypeInstance, typeMapper)
        {
            SerializationMode = TypeSerializationMode.Complex;

            resourceInfo = mappedTypeInstance
                .GetCustomAttributes(typeof (ResourceInfoAttribute), false)
                .OfType<ResourceInfoAttribute>()
                .First();
        }

        public override string Name
        {
            get { return resourceInfo.JsonTypeName; }
        }


        public ResourceInfoAttribute ResourceInfo
        {
            get { return resourceInfo; }
        }


        public override object Create(IDictionary<IPropertyInfo, object> args)
        {
            var instance = Activator.CreateInstance(resourceInfo.PocoType);
            foreach (var kvp in args)
                kvp.Key.Setter(instance, kvp.Value);
            return instance;
        }


        protected override IEnumerable<IPropertyInfo> OnGetProperties()
        {
            // Include properties of all base types too
            var baseInterfaceProperties =
                MappedTypeInstance
                    .GetInterfaces()
                    .Where(x => x.IsInterface && typeof (IClientResource).IsAssignableFrom(x))
                    .SelectMany(x => x.GetProperties())
                    .Select(x => new SharedPropertyInfo(x, TypeMapper));

            return base.OnGetProperties().Concat(baseInterfaceProperties);
        }
    }
}