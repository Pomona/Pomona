using System.Collections.Generic;
namespace Pomona
{
    /// <summary>
    /// A default implementation of IPomonaQuery, only simple querying.
    /// </summary>
    public class PomonaQuery
    {
        public int Page { get; set; }
        public int Count { get; set; }
        public IEnumerable<string> ExpandedPaths { get; set; }
        public IDictionary<string, string> Filters { get; set; }
        
        public PomonaQuery()
        {
        }
    }
}
