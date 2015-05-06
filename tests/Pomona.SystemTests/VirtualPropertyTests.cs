#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class VirtualPropertyTests : ClientTestsBase
    {
        [Test]
        public void Get_type_having_virtual_properties_is_successful()
        {
            var entity = Save(new VirtualPropertyThing() { Number = 25.0 });
            var resource = Client.VirtualPropertyThings.Get(entity.Id);
            Assert.That(resource.Rocky, Is.EqualTo("BALOBA"));
            Assert.That(resource.NumberSquareRoot, Is.EqualTo(5.0));
        }


        [Test]
        public void Post_of_type_having_virtual_properties_is_successful()
        {
            var resource = Client.VirtualPropertyThings.Post(new VirtualPropertyThingForm()
            {
                NumberSquareRoot = 13,
                Rocky = "IS DA MAN"
            });
            Assert.That(resource.Number, Is.EqualTo(13.0 * 13.0));
            Assert.That(resource.NumberSquareRoot, Is.EqualTo(13.0));
            Assert.That(resource.Rocky, Is.EqualTo("IS DA MAN"));
        }


        [Test]
        public void Query_using_virtual_property_in_predicate_is_successful()
        {
            Save(new VirtualPropertyThing()); // Just another thing that will not match predicate.
            var virtualPropThing = Save(new VirtualPropertyThing() { Number = 9.0 });
            var id =
                Client.VirtualPropertyThings.Query().Where(x => x.NumberSquareRoot == 3.0).Select(x => x.Id).Single();
            Assert.That(id, Is.EqualTo(virtualPropThing.Id));
        }
    }
}