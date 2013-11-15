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