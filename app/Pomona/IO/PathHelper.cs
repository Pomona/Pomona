#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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
                throw new ArgumentNullException("pattern");
            if (input == null)
                throw new ArgumentNullException("input");

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