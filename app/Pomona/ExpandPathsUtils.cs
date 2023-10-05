#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pomona
{
    public static class ExpandPathsUtils
    {
        public static HashSet<string> GetExpandedPaths(string expandedPaths)
        {
            return new HashSet<string>((expandedPaths ?? string.Empty).ToLower().Split(',').SelectMany(GetAllSubPaths).Distinct());
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
