using System.Linq;

namespace Pomona.Queries
{
    public interface IQueryExecutor
    {
        PomonaResponse ApplyAndExecute(IQueryable queryable, PomonaQuery query);
    }
}