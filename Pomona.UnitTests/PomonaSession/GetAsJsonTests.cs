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
using System.IO;
using System.Linq;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

using Pomona.Example.Models;

namespace Pomona.UnitTests.PomonaSession
{
    [TestFixture]
    public class GetAsJsonTests : SessionTestsBase
    {
        [Test]
        public void GetNullableJunkWithNull_HasNullValue()
        {
            var jobject = GetNullableJunk(x => !x.Maybe.HasValue);
            jobject.AssertHasPropertyWithNull("maybe");
        }

        [Test]
        public void GetNullableJunkWithValue_HasValue()
        {
            var jobject = GetNullableJunk(x => x.Maybe.HasValue);
            jobject.AssertHasPropertyWithInteger("maybe");
        }


        private JObject GetNullableJunk(Func<JunkWithNullableInt, bool> predicate)
        {
            var junkWithValueId = this.DataSource.List<JunkWithNullableInt>().First(predicate).Id;
            var jobject = JObject.Parse(this.Session.GetAsJson<JunkWithNullableInt>(junkWithValueId, null));
            return jobject;
        }


        private JObject GetCritterAsJson(string expand)
        {
            var stringWriter = new StringWriter();
            Session.GetAsJson<Critter>(FirstCritterId, expand, stringWriter);
            Console.WriteLine("Getting data:\r\n" + stringWriter.ToString());
            var jobject = JObject.Parse(stringWriter.ToString());
            return jobject;
        }


        private JObject GetMusicalCritterAsJson(string expand = null)
        {
            var stringWriter = new StringWriter();
            Session.GetAsJson<Critter>(MusicalCritterId, expand, stringWriter);
            Console.WriteLine("Getting data:\r\n" + stringWriter.ToString());
            var jobject = JObject.Parse(stringWriter.ToString());
            return jobject;
        }


        private JObject GetThingWithCustomListAsJson(string expand)
        {
            var stringWriter = new StringWriter();
            var thing = DataSource.List<ThingWithCustomIList>().First();
            Session.GetAsJson<ThingWithCustomIList>(thing.Id, expand, stringWriter);
            Console.WriteLine("Getting data:\r\n" + stringWriter.ToString());
            var jobject = JObject.Parse(stringWriter.ToString());
            return jobject;
        }


        [Test]
        public void WithCustomList_SerializesOk()
        {
            // Act
            var jobject = GetThingWithCustomListAsJson("thingwithcustomilist.loners!");

            // Assert
            var loners = jobject.AssertHasPropertyWithArray("loners");
        }


        [Test]
        public void WithExpandSetToNull_ReturnsOneLevelByDefault()
        {
            // Act
            var jobject = GetCritterAsJson(null);

            // Assert
            var hat = jobject.AssertHasPropertyWithObject("hat");
            hat.AssertIsReference();
        }


        [Test]
        public void WithExpandSetToNull_ValueObjectIsExpandedByDefault()
        {
            // Act
            var jobject = GetCritterAsJson(null);

            // Assert
            var crazyValue = jobject.AssertHasPropertyWithObject("crazyValue");
            crazyValue.AssertIsExpanded();
            crazyValue.AssertHasProperty("sickness");
        }


        [Test]
        public void WithExpandedHat_HatIsIncluded()
        {
            // Act
            var jobject = GetCritterAsJson("critter.hat");

            // Assert
            var hat = jobject.AssertHasPropertyWithObject("hat");
            var hatType = hat.AssertHasPropertyWithString("hatType");
            Assert.AreEqual(FirstCritter.Hat.HatType, hatType);
        }


        [Test]
        public void WithMusicalCritter_HasSameBaseUriAsCritter()
        {
            var musicalJobject = GetMusicalCritterAsJson();

            var musicalCritterUri = musicalJobject.AssertHasPropertyWithString("_uri");
            Assert.That(musicalCritterUri, Is.EqualTo("http://localhost/critter/" + MusicalCritterId));
        }


        [Test]
        public void WithWeaponsRefExpand_ReturnsArrayOfRefs()
        {
            // Act
            var jobject = GetCritterAsJson("critter.weapons!");

            // Assert
            var weapons = jobject.AssertHasPropertyWithArray("weapons");
            foreach (var jtoken in weapons.Children())
                jtoken.AssertIsReference();
        }
    }
}