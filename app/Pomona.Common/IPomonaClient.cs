using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona.Common
{
    public interface IPomonaClient
    {
        IList<T> Query<T>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null);

        T Get<T>(string uri);
        string GetUriOfType(Type type);
    }
}