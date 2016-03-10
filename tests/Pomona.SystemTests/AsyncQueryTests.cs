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
using System.Threading.Tasks;

using NUnit.Framework;

using Pomona.Common.Linq;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class AsyncQueryTests : ClientTestsBase
    {
        [Test]
        public async Task FutureFirstOrDefault_ReturnsCorrectResult()
        {
            var result = await Client.Critters.Query().Future(x => x.FirstOrDefault(y => y.Id > 0));
            Assert.That(result, Is.Not.Null);
        }


        [Test]
        public async Task FutureMax_ReturnsCorrectResult()
        {
            var expected = CritterEntities.Max(x => x.Id);
            var result = await Client.Critters.Query().Future(x => x.Max(y => y.Id));
            Assert.That(result, Is.EqualTo(expected));
        }


        [Test]
        public async Task ToQueryResultAsync_ReturnsCorrectResult()
        {
            var firstCritterId = CritterEntities.First().Id;
            var result = await Client.Critters.Query().Where(x => x.Id == firstCritterId).ToQueryResultAsync();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(firstCritterId));
        }
    }
}