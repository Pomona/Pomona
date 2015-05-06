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
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;

namespace Pomona
{
    public static class SingularToPluralTranslator
    {
        private static readonly HashSet<char> consonants;
        private static readonly HashSet<string> sibilantEndings;

        public static Dictionary<string, string> IrregularNouns
            ;

        public static Dictionary<string, string> SingularToPluralDict;

        private static readonly string irregularNounsText =
            @"mouse	mice
woman	women
house	houses
ox	oxen
friend	friends
sheep	sheep
door	doors
church	churches
potato	potatoes
cat	cats
cow	cows
mosquito	mosquitoes
man	men
child	children
story	stories
store	stores
watermelon	watermelons
sister-in-law	sisters-in-law
smile	smiles
meter	meters
fox	foxes
cross	crosses
staple	staples
farm	farms
computer	computers
teacher	teachers
country	countries
loaf	loaves
life	lives
chief	chiefs
foot	feet
deer	deer
fish	fish
bus	buses
thirty	thirties
bluff	bluffs
thief	thieves
home	homes
sky	skies
city	cities
waste	wastes
problem	problems
factory	factories
river	rivers
turkey	turkeys
day	days
tax	taxes
night	nights
car	cars
business	businesses
spy	spies
cloud	clouds
farmer	farmers
kitten	kittens
product	products
highway	highways
toy	toys
lady	ladies
gentleman	gentlemen
nose	noses
pepper	peppers
oven	ovens
mess	messes
lion	lions
owl	owls
mountain	mountains
cuff	cuffs
carpet	carpets
light	lights
flower	flowers
line	lines
coin	coins
dollar	dollars
cupful	cupfuls
trout	trout
editor-in-chief	editors-in-chief
penny	pennies
scissors	scissors
pants	pants
inch	inches
goose	geese
tooth	teeth
office	offices
valley	valleys
copy	copies";

        private static readonly string singularToPluralsListText =
            @"mouse	mice
woman	women
house	houses
ox	oxen
friend	friends
sheep	sheep
door	doors
church	churches
potato	potatoes
cat	cats
cow	cows
mosquito	mosquitoes
man	men
child	children
story	stories
store	stores
watermelon	watermelons
sister-in-law	sisters-in-law
smile	smiles
meter	meters
fox	foxes
cross	crosses
staple	staples
farm	farms
computer	computers
teacher	teachers
country	countries
loaf	loaves
life	lives
chief	chiefs
foot	feet
deer	deer
fish	fish
bus	buses
thirty	thirties
bluff	bluffs
thief	thieves
home	homes
sky	skies
city	cities
waste	wastes
problem	problems
factory	factories
river	rivers
turkey	turkeys
day	days
tax	taxes
night	nights
car	cars
business	businesses
spy	spies
cloud	clouds
farmer	farmers
kitten	kittens
product	products
highway	highways
toy	toys
lady	ladies
gentleman	gentlemen
nose	noses
pepper	peppers
oven	ovens
mess	messes
lion	lions
owl	owls
mountain	mountains
cuff	cuffs
carpet	carpets
light	lights
flower	flowers
line	lines
coin	coins
dollar	dollars
cupful	cupfuls
trout	trout
editor-in-chief	editors-in-chief
penny	pennies
scissors	scissors
pants	pants
inch	inches
goose	geese
tooth	teeth
office	offices
valley	valleys
copy	copies";


        static SingularToPluralTranslator()
        {
            SingularToPluralDict = CreateDictionaryFromText(singularToPluralsListText);
            IrregularNouns = CreateDictionaryFromText(irregularNounsText);

            consonants = new HashSet<char>("bcdfghjklmnpqrstvxz".ToCharArray());
            sibilantEndings = new HashSet<string> { "s", "sh", "ch", "x", "z" };
        }


        public static string CamelCaseToPlural(string camelCaseWord)
        {
            // step 1: split up into words
            var parts = NameUtils.GetCamelCaseParts(camelCaseWord).ToArray();
            // step 2: change last word to plural
            parts[parts.Length - 1] = ToPlural(parts[parts.Length - 1]).CapitalizeFirstLetter();
            // step 3: rejoin
            return string.Concat(parts);
        }


        public static bool IsIrregular(string noun)
        {
            return IrregularNouns.ContainsKey(noun.ToLower());
        }


        public static string ToPlural(string noun)
        {
            noun = noun.ToLower();
            if (IrregularNouns.ContainsKey(noun))
                return IrregularNouns[noun];

            return ToPluralNoIrregular(noun);
        }


        public static string ToPluralNoIrregular(string noun)
        {
            // check if word ends in sibilant sound
            if (sibilantEndings.Any(x => noun.EndsWith(x)))
                return noun + "es";

            if (noun.EndsWith("y"))
                return noun.Substring(0, noun.Length - 1) + "ies";

            if (noun.EndsWith("f"))
                return noun.Substring(0, noun.Length - 1) + "ves";

            if (noun.EndsWith("fe"))
                return noun.Substring(0, noun.Length - 2) + "ves";

            return noun + "s";
        }


        private static Dictionary<string, string> CreateDictionaryFromText(string text)
        {
            return text
                .Replace("\r", "")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split("\t ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);
        }


        private static bool IsConsonant(char c)
        {
            return consonants.Contains(c);
        }


        private static bool IsVowel(char c)
        {
            return char.IsLetter(c) && !consonants.Contains(c);
        }
    }
}