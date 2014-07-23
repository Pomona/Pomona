#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
        #region Setup/Teardown

        public override void SetUp()
        {
            base.SetUp();
            CreateTestData();
        }

        #endregion

        public void CreateTestData()
        {
            var galaxy = new Galaxy() { Name = "milkyway" };
            var planetarySystem = new PlanetarySystem() { Name = "solar", Galaxy = galaxy };
            galaxy.PlanetarySystems.Add(planetarySystem);
            var planet = new Planet() { Name = "earth", PlanetarySystem = planetarySystem };
            planetarySystem.Planets.Add(planet);
            Save(galaxy);
        }


        private IPlanetarySystem GetPlanetarySystemResource()
        {
            return Client.Galaxies.Query().First().PlanetarySystems.First();
        }


        [Test]
        public void ChildResourcesGetsCorrectUrl()
        {
            var galaxy = Client.Galaxies.Query().Expand(x => x.PlanetarySystems.Expand(y => y.Planets)).First();
            var planet = galaxy.PlanetarySystems.First().Planets.First();
            var planetUri = ((IHasResourceUri)planet).Uri;

            Assert.That(planetUri, Is.EqualTo("http://test/galaxies/milkyway/planetary-systems/solar/planets/earth"));
            Client.Get<IPlanet>(planetUri);
        }


        [Test]
        public void GetPlanetarySystem_SingleChildResourceHasCorrectUrl()
        {
            var planetarySystem = GetPlanetarySystemResource();
            Assert.That(planetarySystem.Star, Is.TypeOf<StarLazyProxy>());
            var starUrl = ((IHasResourceUri)planetarySystem.Star).Uri;
            Assert.That(starUrl, Is.EqualTo(((IHasResourceUri)planetarySystem).Uri + "/star"));
        }


        [Test]
        public void GetStarOfPlanetarySystem_IsSuccessful()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var starUrl = ((IHasResourceUri)planetarySystem).Uri + "/star";
            var star = Client.Get<IStar>(starUrl);
            Assert.That(star, Is.Not.Null);
            Assert.That(star.Name, Is.EqualTo("Sun"));
        }

        [Test]
        public void PatchStarOfPlanetarySystem_IsSuccessful()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var starUrl = ((IHasResourceUri)planetarySystem).Uri + "/star";
            var star = Client.Get<IStar>(starUrl);
            var patchedStar = Client.Patch(star, s => s.Name = "Sol");
            Assert.That(patchedStar.Name, Is.EqualTo("Sol"));
        }


        [Test]
        public void PatchPlanetarySystemPostDeletePlanetFromChildRepository_IsSuccessful()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var planetToDelete = planetarySystem.Planets.First();
            var patchedPlanetarySystem = Client.Patch(planetarySystem,
                x => x.Planets.Delete(planetToDelete));
            Assert.That(patchedPlanetarySystem.Planets.ToList().Select(x => x.Name),
                Is.Not.Contains(planetToDelete.Name));
        }


        [Test]
        public void PatchPlanetarySystemPostPlanetToChildRepository_IsSuccessful()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var patchedPlanetarySystem = Client.Patch(planetarySystem,
                x => x.Planets.Post(new PlanetForm() { Name = "PostedViaPatch" }));
            Assert.That(patchedPlanetarySystem.Planets.ToList().Select(x => x.Name), Contains.Item("PostedViaPatch"));
        }


        [Test]
        public void PostMoonToPlanet_UsingPatch_IsSuccessful()
        {
            var planet = GetPlanetarySystemResource().Planets.First();
            var patchedPlanet = Client.Patch(planet, p => p.Moons.Add(new MoonForm() { Name = "A new moon" }));
            Assert.That(patchedPlanet.Moons.Any(x => x.Name == "A new moon"), Is.True);
        }


        [Test]
        public void PostPlanetToPlanetarySystem_IsSuccessful()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var planet =
                planetarySystem.Planets.Post(new PlanetForm()
                {
                    Name = "Jupiter",
                    Moons = { new MoonForm() { Name = "jalla" } }
                });
            Assert.That(planet.PlanetarySystem.Id, Is.EqualTo(planetarySystem.Id));
        }


        [Test]
        public void
            PostPlanetToPlanetarySystem_UsingGenericPostOverloadAcceptingActionLambda_WithModifiedEtagOnParent_ThrowsPreconditionFailedException
            ()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var planetarySystemEntity = Repository.Query<PlanetarySystem>().First(x => x.Id == planetarySystem.Id);
            planetarySystemEntity.ETag = "MODIFIED_SINCE_LAST_QUERY";
            Assert.Throws<PreconditionFailedException>(
                () => planetarySystem.Planets.Post<IPlanet>(planetForm =>
                {
                    planetForm.Moons.Add(new MoonForm() { Name = "jalla" });
                    planetForm.Name = "Jupiter";
                }));
        }


        [Test]
        public void
            PostPlanetToPlanetarySystem_UsingPostOverloadAcceptingActionLambda_WithModifiedEtagOnParent_ThrowsPreconditionFailedException
            ()
        {
            CreateTestData();
            var planetarySystem = GetPlanetarySystemResource();
            var planetarySystemEntity = Repository.Query<PlanetarySystem>().First(x => x.Id == planetarySystem.Id);
            planetarySystemEntity.ETag = "MODIFIED_SINCE_LAST_QUERY";
            Assert.Throws<PreconditionFailedException>(
                () => planetarySystem.Planets.Post(planetForm =>
                {
                    planetForm.Moons.Add(new MoonForm() { Name = "jalla" });
                    planetForm.Name = "Jupiter";
                }));
        }


        [Test]
        public void
            PostPlanetToPlanetarySystem_UsingPostOverloadAcceptingForm_WithModifiedEtagOnParent_ThrowsPreconditionFailedException
            ()
        {
            var planetarySystem = GetPlanetarySystemResource();
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