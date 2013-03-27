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

using System.Linq;
using Critters.Client;
using NUnit.Framework;
using Pomona.Common.Linq;
using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class PatchTests : ClientTestsBase
    {
        [Category("TODO")]
        [Test(Description = "TODO: Implement proper patch functionality.")]
        public void PatchCritter_AddNewFormToList()
        {
            var critter = Save(new Critter());
            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.Weapons.Add(new WeaponForm
                             {
                                 Price = 3.4m,
                                 Model = new WeaponModelForm {Name = "balala"},
                                 Strength = 3.5
                             }));

            Assert.That(critter.Weapons, Has.Count.EqualTo(1));

            Assert.Fail("TEST NOT FINISHED");
        }

        [Category("TODO")]
        [Test(Description = "Must refactor PATCH functionality to support this..")]
        public void PatchCritter_UpdateReferenceProperty_UsingValueFromFirstLazyMethod()
        {
            var hat = Save(new Hat {Style = "Gangnam Style 1234"});
            var critter = Save(new Critter());
            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.Hat = client.Query<IHat>().Where(y => y.Style == "Gangnam Style 1234").FirstLazy());

            Assert.That(critter.Hat, Is.EqualTo(hat));
        }

        [Test]
        public void PatchCritter_UpdateStringProperty()
        {
            var critter = Save(new Critter());
            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.Name = "NewName");

            Assert.That(critter.Name, Is.EqualTo("NewName"));
        }

        [Test]
        public void PatchMusicalInheritedCritter_UpdateProperty()
        {
            var critter = Save(new MusicalCritter());
            var resource = client.Query<IMusicalCritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.BandName = "The Patched Sheeps");

            Assert.That(critter.BandName, Is.EqualTo("The Patched Sheeps"));
        }
    }
}