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
    public class DataSourceCollectionRoute : Route, ILiteralRoute
    {
        private readonly DataSourceRootRoute parent;
        private readonly ResourceType resultItemType;


        public DataSourceCollectionRoute(DataSourceRootRoute parent, ResourceType resultItemType)
            : base(0, parent)
        {
            if (resultItemType == null)
                throw new ArgumentNullException(nameof(resultItemType));
            this.parent = parent;
            this.resultItemType = resultItemType;
            ResultType = parent.TypeMapper.FromType(typeof(IEnumerable<>).MakeGenericType(resultItemType));
        }


        public override HttpMethod AllowedMethods => (this.resultItemType.PostAllowed ? HttpMethod.Post : 0) | HttpMethod.Get;

        public override TypeSpec InputType => this.parent.TypeMapper.FromType(typeof(void));

        public override TypeSpec ResultItemType => this.resultItemType;

        public override TypeSpec ResultType { get; }


        protected override IEnumerable<Route> LoadChildren()
        {
            return new GetByIdRoute(this.resultItemType, this, this.resultItemType.AllowedMethods | HttpMethod.Post).WrapAsArray();
        }


        protected override bool Match(string pathSegment)
        {
            return String.Equals(pathSegment, MatchValue, StringComparison.InvariantCultureIgnoreCase);
        }


        protected override string PathSegmentToString()
        {
            return this.resultItemType.UrlRelativePath;
        }


        public string MatchValue => this.resultItemType.UrlRelativePath;
    }
}

