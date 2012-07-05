using System.Collections.Generic;
using System.Linq;

namespace Pomona
{
    public static class ExpandPathsUtils
    {
        public static HashSet<string> GetExpandedPaths(string expandedPaths)
        {
            return new HashSet<string>(expandedPaths.Split(',').SelectMany(GetAllSubPaths).Distinct());
        }


        private static IEnumerable<string> GetAllSubPaths(string s)
        {
            int lastDotIndex;
            while ((lastDotIndex = s.LastIndexOf('.')) != -1)
            {
                yield return s;
                s = s.Substring(0, lastDotIndex);
            }

            yield return s;
        }
        
    }
}