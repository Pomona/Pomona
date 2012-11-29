#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common
{
    public static class NameUtils
    {
        public static string LowercaseFirstLetter(this string word)
        {
            if (word.Length == 0)
                return word;

            return word.Substring(0, 1).ToLower() + word.Substring(1);
        }

        public static string CapitalizeFirstLetter(this string word)
        {
            if (word.Length == 0)
                return word;
            return word.Substring(0, 1).ToUpper() + word.Substring(1);
        }


        public static string ConvertCamelCaseToUri(string word)
        {
            var parts = GetCamelCaseParts(word).ToArray();
            return string.Join((string)"-", (IEnumerable<string>)parts.Select(x => x.ToLower()));
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