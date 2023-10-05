#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    /// <summary>
    /// Tests for generated assembly
    /// </summary>
    [TestFixture]
    public class CritterTests : ClientTestsBase
    {
        [Test]
        public void GetMusicalCritter()
        {
            var musicalCritterId = CritterEntities.OfType<MusicalCritter>().First().Id;

            var musicalCritter = Client.Get<ICritter>(BaseUri + "critters/" + musicalCritterId);

            Assert.That(musicalCritter, Is.AssignableTo<IMusicalCritter>());
        }


        [Test]
        public void GetWeaponsLazy_FromCritter()
        {
            var critter = Client.Critters.First();
            Assert.False(critter.Weapons.IsLoaded());
            var weapons = critter.Weapons.ToList();
            Assert.True(critter.Weapons.IsLoaded());
        }


        [Test]
        public void UsesCustomGetterForAbsoluteFileUrl()
        {
            var critter = Client.Critters.First();
            Assert.That(critter.AbsoluteImageUrl, Is.EqualTo("http://test:80/photos/the-image.png"));
        }


        [Test]
        public void UsesCustomSetterForAbsoluteFileUrl_OnPatch()
        {
            var critter = Client.Critters.First();
            critter = Client.Critters.Patch(critter, x => x.AbsoluteImageUrl = "http://test:80/new-image.png");
            Assert.That(critter.AbsoluteImageUrl, Is.EqualTo("http://test:80/new-image.png"));
            var critterEntity = CritterEntities.First(x => x.Id == critter.Id);
            Assert.That(critterEntity.RelativeImageUrl, Is.EqualTo("/new-image.png"));
        }


        [Test]
        public void UsesCustomSetterForAbsoluteFileUrl_OnPost()
        {
            var critter = Client.Critters.Post(new CritterForm() { AbsoluteImageUrl = "http://test:80/holala.png" });
            Assert.That(critter.AbsoluteImageUrl, Is.EqualTo("http://test:80/holala.png"));
            var critterEntity = CritterEntities.First(x => x.Id == critter.Id);
            Assert.That(critterEntity.RelativeImageUrl, Is.EqualTo("/holala.png"));
        }
    }
}

