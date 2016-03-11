#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Fetcher
{
    public interface IBatchFetchDriver
    {
        PropertyInfo GetIdProperty(Type type);
        IEnumerable<PropertyInfo> GetProperties(Type type);
        bool IsLoaded(object obj);
        bool IsManyToOne(PropertyInfo prop);
        bool PathIsExpanded(string path, PropertyInfo property);


        void PopulateCollections<TParentEntity, TCollectionElement>(
            IEnumerable<KeyValuePair<TParentEntity, IEnumerable<TCollectionElement>>> bindings,
            PropertyInfo property,
            Type elementType);


        IQueryable<TEntity> Query<TEntity>();
    }
}