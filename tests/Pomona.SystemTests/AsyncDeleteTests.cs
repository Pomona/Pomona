#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class AsyncDeleteTests : ClientTestsBase
    {
        [Test]
        public async Task DeleteAsync_IsSuccessful()
        {
            var handledThingId = Save(new HandledThing()).Id;
            await Client.HandledThings.DeleteAsync(Client.HandledThings.GetLazy(handledThingId));
            Assert.That(Repository.Query<HandledThing>().Any(x => x.Id == handledThingId), Is.False);
        }
    }
}