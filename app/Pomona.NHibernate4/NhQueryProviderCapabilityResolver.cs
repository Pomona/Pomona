#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Reflection;

using NHibernate;

using Pomona.Routing;

namespace Pomona.NHibernate4
{
    public class NhQueryProviderCapabilityResolver : IQueryProviderCapabilityResolver
    {
        private readonly ISessionFactory factory;


        public NhQueryProviderCapabilityResolver(ISessionFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
        }


        public bool PropertyIsMapped(PropertyInfo propertyInfo)
        {
            var cm = this.factory.GetClassMetadata(propertyInfo.ReflectedType);
            if (cm == null)
                return false;
            return (cm.HasIdentifierProperty && cm.IdentifierPropertyName == propertyInfo.Name)
                   || cm.PropertyNames.Contains(propertyInfo.Name);
        }
    }
}