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
using Pomona.Common;
using Pomona.Common.Linq;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class AsyncClientQueryTests : ClientTestsBase
    {
        public override bool UseSelfHostedHttpServer
        {
            get { return true; }
        }

        [Test]
        public void GetAsyncUsingUri_ReturnsResource()
        {
            // First get the uri a way we know works:
            var critterUri = ((IHasResourceUri) client.Critters.Query().FirstLazy()).Uri;

            var asyncCritter = client.GetAsync<ICritter>(critterUri).Result;

            var expected = CritterEntities.First();
            Assert.That(asyncCritter, Is.Not.Null);
            Assert.That(asyncCritter.Id, Is.EqualTo(expected.Id));
            Assert.That(asyncCritter.Name, Is.EqualTo(expected.Name));
        }

        [Test]
        public void Query_ToListAsync_ReturnsResources()
        {
            var expectedCritters = CritterEntities.Where(x => x.Id % 3 == 0).Take(5).ToList();
            var fetchedCritters = client.Critters.Query().Where(x => x.Id % 3 == 0).Take(5).ToListAsync().Result;

            Assert.That(fetchedCritters.Select(x => x.Id), Is.EquivalentTo(expectedCritters.Select(x => x.Id)));
        }
    }
}