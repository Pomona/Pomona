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
            var stringWriter = new StringWriter();
            Session.GetAsJson<Critter>(CritterId, null, stringWriter);
            var jobject = JObject.Parse(stringWriter.ToString());

            // Assert
            JObject hat = jobject.AssertHasPropertyWithObject("hat");
            hat.AssertIsReference();
        }
    }
}
