#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
        /// <summary>
        /// Is this property the primary property for a child resource?
        /// </summary>
        internal static bool IsPrimaryChildResourceProperty(this PropertySpec property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var resourceType = property.PropertyType.GetItemType() as ResourceType;
            if (resourceType == null)
                return false;

            return property == resourceType.ParentToChildProperty;
        }


        public static IEnumerable<Route> GetRoutes(this ResourceType resourceType, Route parent)
        {
            if (resourceType == null)
                throw new ArgumentNullException("resourceType");

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
                throw new ArgumentNullException("property");
            var routes = property.PropertyType.Maybe().Switch(
                x =>
                    x.Case<ResourceType>().Then(y => y.GetRoutes(parent)).Case<EnumerableTypeSpec>().Then(
                        y => new GetByIdRoute((ResourceType)y.ItemType, parent, property.ItemAccessMode).WrapAsEnumerable())).OrDefault(
                            Enumerable.Empty<Route>);
            return routes;
        }
    }
}