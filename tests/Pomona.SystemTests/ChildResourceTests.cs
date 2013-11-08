using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Example.Models.Existence;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class ChildResourceTests : ClientTestsBase
    {
        public void CreateTestData()
        {
            var galaxy = new Galaxy() { Name = "milkyway" };
            var planetarySystem = new PlanetarySystem() { Name = "solar", Galaxy = galaxy};
            galaxy.PlanetarySystems.Add(planetarySystem);
            var planet = new Planet() { Name = "earth", PlanetarySystem = planetarySystem };
            planetarySystem.Planets.Add(planet);
            Save(galaxy);
        }

        [Test]
        public void ChildResourcesGetsCorrectUrl()
        {
            CreateTestData();
            var galaxy = client.Galaxies.Query().Expand(x => x.PlanetarySystems.Expand(y => y.Planets)).First();
            var planet = galaxy.PlanetarySystems.First().Planets.First();
            client.Get<IPlanet>(((IHasResourceUri)planet).Uri);

            Assert.Fail("Test not finished");
        }
    }
}