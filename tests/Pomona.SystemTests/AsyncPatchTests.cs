#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Pomona.Common;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class AsyncPatchTests : ClientTestsBase
    {
        [Test]
        public async Task Patch_IsSuccessful()
        {
            var critterEntity = Client.Critters.First();
            var updatedCritter = await Client.PatchAsync(critterEntity, c => c.Name = "UPDATED!");
            Assert.That(updatedCritter.Name, Is.EqualTo("UPDATED!"));
        }
    }
}