#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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


        public override HttpMethod AllowedMethods
        {
            get { return Property.AccessMode; }
        }

        public override TypeSpec InputType
        {
            get { return Property.DeclaringType; }
        }

        public ResourceProperty Property { get; }

        public override TypeSpec ResultType
        {
            get { return Property.PropertyType; }
        }


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


        public string MatchValue
        {
            get { return Property.UriName; }
        }
    }
}