#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Threading.Tasks;

namespace Pomona.Queries
{
    public class DefaultQueryExecutor : IQueryExecutor
    {
        public virtual Task<PomonaResponse> ApplyAndExecute(IQueryable queryable, PomonaQuery query)
        {
            return Task.FromResult(query.ApplyAndExecute(queryable));
        }
    }
}