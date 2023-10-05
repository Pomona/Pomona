#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class ResourcePropertyRoute : Route, ILiteralRoute
    {
        public ResourcePropertyRoute(ResourceProperty property, Route parent)
            : base(0, parent)
        {
            Property = property;
        }


        public override HttpMethod AllowedMethods => Property.AccessMode;

        public override TypeSpec InputType => Property.DeclaringType;

        public ResourceProperty Property { get; }

        public override TypeSpec ResultType => Property.PropertyType;


        protected override IEnumerable<Route> LoadChildren()
        {
            return Property.GetRoutes(this);
        }


        protected override bool Match(string pathSegment)
        {
            return string.Equals(pathSegment, Property.UriName, StringComparison.InvariantCultureIgnoreCase);
        }


        protected override string PathSegmentToString()
        {
            return Property.UriName;
        }


        public string MatchValue => Property.UriName;
    }
}

