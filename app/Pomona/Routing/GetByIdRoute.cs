#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class GetByIdRoute : Route
    {
        private readonly ResourceType resultItemType;


        public GetByIdRoute(ResourceType resultItemType, Route parent, HttpMethod allowedMethods)
            : base(10, parent)
        {
            if (resultItemType == null)
                throw new ArgumentNullException(nameof(resultItemType));
            this.resultItemType = resultItemType;
            AllowedMethods = allowedMethods;
            IdProperty = this.resultItemType.PrimaryId;
            if (IdProperty == null)
                throw new ArgumentException("Resource in collection needs to have a primary id.");
        }


        public override HttpMethod AllowedMethods { get; }

        public StructuredProperty IdProperty { get; }

        public override TypeSpec InputType => this.resultItemType;

        public override TypeSpec ResultType => this.resultItemType;


        protected override IEnumerable<Route> LoadChildren()
        {
            return this.resultItemType.GetRoutes(this);
        }


        protected override bool Match(string pathSegment)
        {
            object parsedId;
            return pathSegment.TryParse(IdProperty.PropertyType, out parsedId);
        }


        protected override string PathSegmentToString()
        {
            return $"{{{IdProperty.JsonName}}}";
        }
    }
}

