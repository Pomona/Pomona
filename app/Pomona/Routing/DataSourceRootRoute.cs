#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class DataSourceRootRoute : Route
    {
        public DataSourceRootRoute(TypeMapper typeMapper, Type dataSource)
            : base(0, null)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            var dataSourceInterface = typeof(IPomonaDataSource);
            dataSource = dataSource ?? dataSourceInterface;

            if (!dataSourceInterface.IsAssignableFrom(dataSource))
            {
                throw new ArgumentException($"dataSourceType must be castable to {dataSourceInterface.FullName}");
            }

            TypeMapper = typeMapper;
            DataSource = dataSource;
        }


        public override HttpMethod AllowedMethods => HttpMethod.Get;

        public override TypeSpec InputType => TypeMapper.FromType(typeof(void));

        public override TypeSpec ResultType => TypeMapper.FromType(typeof(IDictionary<string, object>));

        internal Type DataSource { get; }

        internal TypeMapper TypeMapper { get; }


        protected override IEnumerable<Route> LoadChildren()
        {
            return GetRootResourceBaseTypes().Select(x => new DataSourceCollectionRoute(this, x));
        }


        protected override bool Match(string pathSegment)
        {
            throw new NotSupportedException("Root route only supports MatchChildren()");
        }


        protected override string PathSegmentToString()
        {
            return string.Empty;
        }


        internal IEnumerable<ResourceType> GetRootResourceBaseTypes()
        {
            return TypeMapper.SourceTypes
                             .OfType<ResourceType>()
                             .Where(x => x.IsUriBaseType && x.ParentResourceType == null);
        }
    }
}