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
using System.Linq;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

using Pomona.Example.Models;

namespace Pomona.UnitTests.PomonaSession
{
    [TestFixture]
    public class GetAsJsonTests : SessionTestsBase
    {
        private JObject GetAsJson<T>(int id, string expand = null)
            where T : EntityBase
        {
            var transformedType = (TransformedType)Session.TypeMapper.GetClassMapping<T>();
            var jsonString = Session.GetAsJson(transformedType, id, expand);
            Console.WriteLine("Object converted to JSON:\r\n" + jsonString);
            return JObject.Parse(jsonString);
        }


        private JObject SaveAndGetBackAsJson<T>(T entity, string expand = null)
            where T : EntityBase
        {
            DataSource.Save(entity);
            return GetAsJson<T>(entity.Id);
        }


        private JObject GetThingWithCustomListAsJson(string expand)
        {
            var thing = DataSource.List<ThingWithCustomIList>().First();
            return GetAsJson<ThingWithCustomIList>(thing.Id, expand);
        }


        [Test]
        public void GetDictionaryContainerWithItemSet_HasDictionaryAsMapInJson()
        {
            var dictionaryContainer = new DictionaryContainer();
            dictionaryContainer.Map["cow"] = "moo";
            var jobject = SaveAndGetBackAsJson(dictionaryContainer, "map");
            var mapJobject = jobject.AssertHasPropertyWithObject("map");
            var cowString = mapJobject.AssertHasPropertyWithString("cow");
            Assert.That(cowString, Is.EqualTo("moo"));
        }


        [Test]
        public void GetIntListContainerAsJson()
        {
            var jobject = SaveAndGetBackAsJson(new IntListContainer() { Ints = { 1337 } }, "ints");
            var stringArray = jobject.AssertHasPropertyWithArray("ints");
            Assert.That(stringArray.Children().Select(x => x.Value<int>()), Is.EquivalentTo(new[] { 1337 }));
        }


        [Test]
        public void GetNullableJunkWithNull_HasNullValue()
        {
            var jobject = SaveAndGetBackAsJson(new JunkWithNullableInt() { Maybe = null });
            jobject.AssertHasPropertyWithNull("maybe");
        }


        [Test]
        public void GetNullableJunkWithValue_HasValue()
        {
            var jobject = SaveAndGetBackAsJson(new JunkWithNullableInt() { Maybe = 123 });
            jobject.AssertHasPropertyWithInteger("maybe");
        }


        [Test]
        public void GetStringListContainerAsJson()
        {
            var jobject = SaveAndGetBackAsJson(new StringListContainer() { Strings = { "Doh!" } }, "strings");
            var stringArray = jobject.AssertHasPropertyWithArray("strings");
            Assert.That(stringArray.Children().Select(x => x.Value<string>()), Is.EquivalentTo(new[] { "Doh!" }));
        }


        [Test]
        public void WithColorfulThing_SerializesWebColorAsString()
        {
            var colorfulThing = new ColorfulThing();
            var jobject = SaveAndGetBackAsJson(colorfulThing);
            var value = jobject.AssertHasPropertyWithString("color");
            Assert.That(value, Is.EqualTo(colorfulThing.Color.ToStringConverted()));
        }


        [Test]
        public void WithCustomEnum_SerializesAsEnumValueString()
        {
            var theEnumValue = CustomEnum.Tock;
            var jobject = SaveAndGetBackAsJson(new HasCustomEnum() { TheEnumValue = theEnumValue });
            var jsonEnumValue = jobject.AssertHasPropertyWithString("theEnumValue");
            Assert.That(jsonEnumValue, Is.EqualTo(theEnumValue.ToString()));
        }


        [Test]
        public void WithCustomList_SerializesOk()
        {
            // Act
            var jobject = GetThingWithCustomListAsJson("loners!");

            // Assert
            var loners = jobject.AssertHasPropertyWithArray("loners");
        }


        [Test]
        public void WithEntityThatGotRenamedProperty_HasCorrectPropertyName()
        {
            var propval = "Funky junk";
            var jobject = SaveAndGetBackAsJson(new JunkWithRenamedProperty() { ReallyUglyPropertyName = propval });

            // Assert
            jobject.AssertHasPropertyWithString("beautifulAndExposed");
            jobject.AssertDoesNotHaveProperty("reallyUglyPropertyName");
        }


        [Test]
        public void WithExpandSetToNull_ReturnsOneLevelByDefault()
        {
            // Act
            var jobject = GetAsJson<Critter>(FirstCritterId);

            // Assert
            var hat = jobject.AssertHasPropertyWithObject("hat");
            hat.AssertIsReference();
        }


        [Test]
        public void WithExpandSetToNull_ValueObjectIsExpandedByDefault()
        {
            // Act
            var jobject = GetAsJson<Critter>(FirstCritterId);

            // Assert
            var crazyValue = jobject.AssertHasPropertyWithObject("crazyValue");
            crazyValue.AssertIsExpanded();
            crazyValue.AssertHasProperty("sickness");
        }


        [Test]
        public void WithExpandedHat_HatIsIncluded()
        {
            // Act
            var jobject = GetAsJson<Critter>(FirstCritterId, "hat");

            // Assert
            var hat = jobject.AssertHasPropertyWithObject("hat");
            var hatType = hat.AssertHasPropertyWithString("hatType");
            Assert.AreEqual(FirstCritter.Hat.HatType, hatType);
        }


        [Test]
        public void WithMusicalCritter_HasSameBaseUriAsCritter()
        {
            var musicalJobject = GetAsJson<Critter>(MusicalCritterId);

            var musicalCritterUri = musicalJobject.AssertHasPropertyWithString("_uri");
            Assert.That(musicalCritterUri, Is.EqualTo("http://localhost/critter/" + MusicalCritterId));
        }


        [Test]
        public void WithThingWithUri_ReturnsUrlAsString()
        {
            var theUrlString = "http://bahahaha/";
            var jobject = SaveAndGetBackAsJson(new ThingWithUri() { TheUrl = new Uri(theUrlString) });

            Assert.That(jobject.AssertHasPropertyWithString("theUrl"), Is.EqualTo(theUrlString));
        }


        [Test]
        public void WithWeaponsRefExpand_ReturnsArrayOfRefs()
        {
            // Act
            var jobject = GetAsJson<Critter>(FirstCritterId, "weapons!");

            // Assert
            var weapons = jobject.AssertHasPropertyWithArray("weapons");
            foreach (var jtoken in weapons.Children())
                jtoken.AssertIsReference();
        }
    }
}