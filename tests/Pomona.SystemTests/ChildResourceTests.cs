#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
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
        [Test]
        public void ChildResourcesGetsCorrectUrl()
        {
            var galaxy = Client.Galaxies.Query().Expand(x => x.PlanetarySystems.Expand(y => y.Planets)).First();
            var planet = galaxy.PlanetarySystems.First().Planets.First();
            var planetUri = ((IHasResourceUri)planet).Uri;

            Assert.That(planetUri, Is.EqualTo("http://test/galaxies/milkyway/planetary-systems/solar/planets/earth"));
            Client.Get<IPlanet>(planetUri);
        }


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
        public void GetChildResourceFromProperty_AtNonExistantUrl_ThrowsResourceNotFoundException()
        {
            Client.Get<IGalaxyInfo>("http://test/galaxies/milkyway/info");
            Assert.Throws<Common.Web.ResourceNotFoundException>(
                () => Client.Get<IGalaxyInfo>("http://test/galaxies/nowhere/info"));
        }


        [Test]
        public void GetChildResourceIdentifiedById_AtNonExistantUrl_ThrowsResourceNotFoundException()
        {
            Assert.Throws<Common.Web.ResourceNotFoundException>(
                () => Client.Get<IPlanetarySystem>("http://test/galaxies/nowhere/planetary-systems/nada"));
        }


        [Test]
        public void GetCollectionOfChildResourcesFromProperty_AtNonExistantUrl_ThrowsResourceNotFoundException()
        {
            Assert.Throws<Common.Web.ResourceNotFoundException>(
                () => Client.Get<QueryResult<IPlanetarySystem>>("http://test/galaxies/nowhere/planetary-systems"));
        }


        [Test]
        public void GetGalaxyAtUrlHavingEncodedCharacters_ReturnsResource()
        {
            var resourceName = "ksaj dlkj skdl jsklj ædøs ¤&(";
            Save(new Galaxy() { Name = resourceName });
            var galaxy = Client.Galaxies.Get(resourceName);
            Assert.That(galaxy, Is.Not.Null);
            Assert.That(galaxy.Name, Is.EqualTo(resourceName));
            Console.WriteLine(((IHasResourceUri)galaxy).Uri);
            Assert.That(((IHasResourceUri)galaxy).Uri,
                        Is.EqualTo("http://test/galaxies/ksaj%20dlkj%20skdl%20jsklj%20%C3%A6d%C3%B8s%20%C2%A4%26("));
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
        public void PatchStarOfPlanetarySystem_IsSuccessful()
        {
            var planetarySystem = GetPlanetarySystemResource();
            var starUrl = ((IHasResourceUri)planetarySystem).Uri + "/star";
            var star = Client.Get<IStar>(starUrl);
            var patchedStar = Client.Patch(star, s => s.Name = "Sol");
            Assert.That(patchedStar.Name, Is.EqualTo("Sol"));
        }


        [Test]
        public void PostMoonToPlanet_UsingPatch_IsSuccessful()
        {
            var planet = GetPlanetarySystemResource().Planets.OfType<IPlanet>().First();
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
                () => planetarySystem.Planets.Post<IPlanet>(planetForm =>
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

        #region Setup/Teardown

        public override void SetUp()
        {
            base.SetUp();
            CreateTestData();
        }

        #endregion

        private IPlanetarySystem GetPlanetarySystemResource()
        {
            return Client.Galaxies.Query().First().PlanetarySystems.First();
        }
    }
}