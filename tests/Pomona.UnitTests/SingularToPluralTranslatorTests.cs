#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using NUnit.Framework;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class SingularToPluralTranslatorTests
    {
        [Test]
        public void ToPlural_AddressBecomesAddresses()
        {
            Assert.That(SingularToPluralTranslator.ToPlural("address"), Is.EqualTo("addresses"));
        }


        [Test]
        public void ToPlural_CheckRules()
        {
            var failedTranslationCount = 0;
            var redundantInIrregularDictCount = 0;

            Console.WriteLine("Number of words to translate " + SingularToPluralTranslator.IrregularNouns.Count);

            foreach (var kvp in SingularToPluralTranslator.IrregularNouns)
            {
                var singular = kvp.Key;
                var expectedPlural = kvp.Value;
                var computedPlural = SingularToPluralTranslator.ToPlural(singular);
                if (computedPlural != expectedPlural)
                {
                    Console.WriteLine(
                        "computed \"" + computedPlural + "\"  but expected was \"" + expectedPlural + "\"");
                    failedTranslationCount++;
                }

                var computedPluralIgnoreIrregulars = SingularToPluralTranslator.ToPluralNoIrregular(singular);
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
