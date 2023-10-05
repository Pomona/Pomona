#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Reflection;

namespace Pomona.Routing
{
    public interface IQueryProviderCapabilityResolver
    {
        bool PropertyIsMapped(PropertyInfo propertyInfo);
    }
}

