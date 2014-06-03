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