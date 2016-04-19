#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Reflection;

namespace Pomona.Routing
{
    public class DefaultQueryProviderCapabilityResolver : IQueryProviderCapabilityResolver
    {
        public bool PropertyIsMapped(PropertyInfo propertyInfo)
        {
            return true;
        }
    }
}