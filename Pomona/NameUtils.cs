using System.Collections.Generic;

using System.Linq;

namespace Pomona
{
    public static class NameUtils
    {
        public static string CapitalizeFirstLetter(string word)
        {
            if (word.Length == 0)
                return word;
            return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
        }

        public static string ConvertCamelCaseToUri(string word)
        {
            var parts = GetCamelCaseParts(word).ToArray();
            return string.Join("-", parts.Select(x => x.ToLower()));
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



    }
}