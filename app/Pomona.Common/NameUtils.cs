#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomona.Common
{
    public static class NameUtils
    {
        public static string CapitalizeFirstLetter(this string word)
        {
            if (word.Length == 0)
                return word;
            return word.Substring(0, 1).ToUpper() + word.Substring(1);
        }


        public static string ConvertCamelCaseToUri(string word)
        {
            var parts = GetCamelCaseParts(word).ToArray();
            return string.Join("-", parts.Select(x => x.ToLower()));
        }


        public static string ConvetUriSegmentToCamelCase(string uriSegment)
        {
            var sb = new StringBuilder();
            var nextCharToUpper = true;
            foreach (var c in uriSegment)
            {
                if (c == '-')
                    nextCharToUpper = true;
                else
                {
                    sb.Append(nextCharToUpper ? char.ToUpperInvariant(c) : c);
                    nextCharToUpper = false;
                }
            }
            return sb.ToString();
        }


        public static IEnumerable<string> GetCamelCaseParts(string camelCaseWord)
        {
            var startOfPart = 0;
            for (var i = 0; i < camelCaseWord.Length; i++)
            {
                if (i > 0 && char.IsUpper(camelCaseWord[i]))
                {
                    yield return camelCaseWord.Substring(startOfPart, i - startOfPart);
                    startOfPart = i;
                }
            }

            if (startOfPart < camelCaseWord.Length)
                yield return camelCaseWord.Substring(startOfPart);
        }


        public static string LowercaseFirstLetter(this string word)
        {
            if (word.Length == 0)
                return word;

            return word.Substring(0, 1).ToLower() + word.Substring(1);
        }
    }
}