#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public static class ResourceTypeExtensions
    {
        public static IEnumerable<Route> GetRoutes(this ResourceType resourceType, Route parent)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            var mergedTypes = resourceType.MergedTypes.ToList();
            if (mergedTypes.Any())
            {
                var exposedProps =
                    resourceType.Properties.Concat(
                        mergedTypes.SelectMany(x => x.Properties.Where(y => y.DeclaringType == x))).Where(
                            x => x.ExposedOnUrl).ToList();
                //if (
                //    exposedProps.GroupBy(x => x.UriName, StringComparer.InvariantCultureIgnoreCase).Any(
                //        x => x.Count() > 1))
                //{
                //    throw new NotImplementedException(
                //        "Does not support subclassing with confliciting property names, yet");
                //}
                return exposedProps.Select(x => new ResourcePropertyRoute(x, parent));
            }
            return resourceType.Properties.Where(x => x.ExposedOnUrl).Select(x => new ResourcePropertyRoute(x, parent));
        }


        public static IEnumerable<Route> GetRoutes(this StructuredProperty property, Route parent)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var routes = property.PropertyType.Maybe().Switch(
                x =>
                    x.Case<ResourceType>().Then(y => y.GetRoutes(parent)).Case<EnumerableTypeSpec>().Then(
                        y => new GetByIdRoute((ResourceType)y.ItemType, parent, property.ItemAccessMode).WrapAsEnumerable())).OrDefault(
                            Enumerable.Empty<Route>);
            return routes;
        }


        /// <summary>
        /// Is this property the primary property for a child resource?
        /// </summary>
        internal static bool IsPrimaryChildResourceProperty(this PropertySpec property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var resourceType = property.PropertyType.GetItemType() as ResourceType;
            if (resourceType == null)
                return false;

            return property == resourceType.ParentToChildProperty;
        }
    }
}

