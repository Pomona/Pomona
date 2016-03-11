#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pomona.IO
{
    public static class PathHelper
    {
        private static readonly char[] escapeTable;


        static PathHelper()
        {
            var escaped = "\n\r\t\f #$()*+.?[]\\^{|}";
            var escapeCodes = "nrtf #$()*+.?[]\\^{|}";
            escapeTable = Enumerable
                .Range(0, 128)
                .Select(x =>
                {
                    var c = (char)x;
                    var escapeIndex = escaped.IndexOf(c);
                    if (escapeIndex == -1)
                        return '\0';
                    return escapeCodes[escapeIndex];
                })
                .ToArray();
        }


        public static bool MatchUrlPathSpec(string input, string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return Regex.IsMatch(input, SpecToRegex(pattern));
        }


        public static string SpecToRegex(string spec)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('^');
            var len = spec.Length;
            for (int i = 0; i < len; i++)
            {
                var c = spec[i];
                if (c == '*')
                {
                    var ip1 = i + 1;
                    if (ip1 < len)
                    {
                        var cn = spec[ip1];
                        if (cn == '*')
                        {
                            // Double asterisk, greedy match
                            sb.Append(".*");
                        }
                        else
                        {
                            sb.Append("[^/");
                            AppendRegexEscapedChar(sb, cn);
                            sb.Append("]*");
                        }
                    }
                    else
                        sb.Append("[^/]*");
                }
                else
                    AppendRegexEscapedChar(sb, c);
            }
            sb.Append("$");
            return sb.ToString();
        }


        private static void AppendRegexEscapedChar(StringBuilder sb, char c)
        {
            char escCode;
            if (c < 128 && (escCode = escapeTable[c]) > '\0')
            {
                sb.Append('\\');
                sb.Append(escCode);
            }
            else
                sb.Append(c);
        }
    }
}