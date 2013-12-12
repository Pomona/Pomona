#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Common.Web;
using Pomona.Example.Models.Existence;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class ChildResourceTests : ClientTestsBase
    {
        public void CreateTestData()
        {
            var galaxy = new Galaxy() { Name = "milkyway" };
            var planetarySystem = new PlanetarySystem() { Name = "solar", Galaxy = galaxy };
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
            var planetUri = ((IHasResourceUri)planet).Uri;

            Assert.That(planetUri, Is.EqualTo("http://test/galaxies/milkyway/planetary-systems/solar/planets/earth"));
            client.Get<IPlanet>(planetUri);
        }


        [Test]
        public void PostPlanetToPlanetarySystem_IsSuccessful()
        {
            CreateTestData();
            var planetarySystem = client.Galaxies.Query().First().PlanetarySystems.First();
            var planet =
                planetarySystem.Planets.Post(new PlanetForm()
                {
                    Name = "Jupiter",
                    Moons = { new MoonForm() { Name = "jalla" } }
                });
            Assert.That(planet.PlanetarySystem.Id, Is.EqualTo(planetarySystem.Id));
        }


        [Test]
        public void PostPlanetToPlanetarySystem_WithModifiedEtagOnParent()
        {
            CreateTestData();
            var planetarySystem = client.Galaxies.Query().First().PlanetarySystems.First();
            var planetarySystemEntity = Repository.Query<PlanetarySystem>().First(x => x.Id == planetarySystem.Id);
            planetarySystemEntity.ETag = "MODIFIED_SINCE_LAST_QUERY";
            Assert.Throws<PreconditionFailedException>(
                () =>
                    planetarySystem.Planets.Post(new PlanetForm()
                    {
                        Name = "Jupiter",
                        Moons = { new MoonForm() { Name = "jalla" } }
                    }));
        }
    }
}