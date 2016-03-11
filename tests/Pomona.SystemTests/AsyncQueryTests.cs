#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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