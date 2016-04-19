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
    public class RootRoute : Route
    {
        private readonly ResourceType resultItemType;


        public RootRoute(ResourceType resultItemType)
            : base(0, null)
        {
            this.resultItemType = resultItemType;
        }


        public override HttpMethod AllowedMethods
        {
            get { return HttpMethod.Get; }
        }

        public override TypeSpec InputType
        {
            get { return this.resultItemType.TypeResolver.FromType(typeof(void)); }
        }

        public override TypeSpec ResultType
        {
            get { return this.resultItemType; }
        }


        protected override IEnumerable<Route> LoadChildren()
        {
            return this.resultItemType.GetRoutes(this);
        }


        protected override bool Match(string pathSegment)
        {
            throw new NotSupportedException("Root route only supports MatchChildren()");
        }


        protected override string PathSegmentToString()
        {
            return string.Empty;
        }
    }
}