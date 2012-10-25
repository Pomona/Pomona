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

using System;

using NUnit.Framework;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class SingularToPluralTranslatorTests
    {
        [Test]
        public void ToPlural_CheckRules()
        {
            var translator = new SingularToPluralTranslator();
            var failedTranslationCount = 0;
            var redundantInIrregularDictCount = 0;

            Console.WriteLine("Number of words to translate " + SingularToPluralTranslator.SingularToPluralDict.Count);

            foreach (var kvp in SingularToPluralTranslator.SingularToPluralDict)
            {
                var singular = kvp.Key;
                var expectedPlural = kvp.Value;
                var computedPlural = translator.ToPlural(singular);
                if (computedPlural != expectedPlural)
                {
                    Console.WriteLine(
                        "computed \"" + computedPlural + "\"  but expected was \"" + expectedPlural + "\"");
                    failedTranslationCount++;
                }

                var computedPluralIgnoreIrregulars = translator.ToPluralNoIrregular(singular);
                if (SingularToPluralTranslator.IrregularNouns.ContainsKey(singular)
                    && computedPluralIgnoreIrregulars == computedPlural)
                {
                    Console.WriteLine(
                        singular + " to " + computedPlural
                        + " was in irregular dict, but is covered by regular rules.. No worry!");
                    redundantInIrregularDictCount++;
                }
            }

            Console.WriteLine(
                "Number of irregular nouns in dict: " + SingularToPluralTranslator.IrregularNouns.Count
                + ", redundant count: " + redundantInIrregularDictCount);

            Assert.That(failedTranslationCount, Is.EqualTo(0));
        }
    }
}