#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

namespace Pomona.Queries
{
    public class DefaultQueryExecutor : IQueryExecutor
    {
        public virtual PomonaResponse ApplyAndExecute(IQueryable queryable, PomonaQuery query)
        {
            return query.ApplyAndExecute(queryable);
        }
    }
}