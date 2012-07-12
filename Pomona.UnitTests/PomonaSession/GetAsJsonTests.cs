using System;
using System.IO;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Pomona.Example.Models;

namespace Pomona.UnitTests.PomonaSession
{
    [TestFixture]
    public class GetAsJsonTests : SessionTestsBase
    {
        [Test]
        public void WithExpandSetToNull_ReturnsOneLevelByDefault()
        {
            // Act
            var jobject = GetCritterAsJson(null);

            // Assert
            JObject hat = jobject.AssertHasPropertyWithObject("hat");
            hat.AssertIsReference();
        }

        [Test]
        public void WithExpandedHat_HatIsIncluded()
        {
            // Act
            var jobject = GetCritterAsJson("critter.hat");

            // Assert
            JObject hat = jobject.AssertHasPropertyWithObject("hat");
            var hatType = hat.AssertHasPropertyWithString("hatType");
            Assert.AreEqual(FirstCritter.Hat.HatType, hatType);
        }

        private JObject GetCritterAsJson(string expand)
        {
            var stringWriter = new StringWriter();
            Session.GetAsJson<Critter>(FirstCritterId, expand, stringWriter);
            var jobject = JObject.Parse(stringWriter.ToString());
            return jobject;
        }
    }
}
