#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Nancy.Bootstrapper;
using Nancy.Routing;

using Pomona.Queries;
using Pomona.Routing;

namespace Pomona.Plumbing
{
    public class PomonaRegistrations : IRegistrations
    {
        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                yield return
                    new CollectionTypeRegistration(typeof(IRouteMetadataProvider),
                                                   new[] { typeof(PomonaRouteMetadataProvider) });
            }
        }

        public IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get { yield break; }
        }

        public IEnumerable<TypeRegistration> TypeRegistrations => new[]
        {
            new TypeRegistration(typeof(IPomonaModuleConfigurationBinder),
                                 typeof(PomonaModuleConfigurationBinder), Lifetime.Singleton),
            new TypeRegistration(typeof(IQueryExecutor), typeof(DefaultQueryExecutor), Lifetime.Transient)
        };
    }
}
