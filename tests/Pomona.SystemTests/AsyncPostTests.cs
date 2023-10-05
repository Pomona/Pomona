#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Threading.Tasks;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class AsyncPostTests : ClientTestsBase
    {
        [Test]
        public async Task PostAsync_ExtensionMethod_UsingAction_WithNoOptionSpecified_IsSuccessful()
        {
            var critter = await Client.Critters.PostAsync(c => c.Name = "Blah");
            Assert.That(critter.Name, Is.EqualTo("Blah"));
        }


        [Test]
        public async Task PostAsync_UsingAction_IsSuccessful()
        {
            var result = await Client.Critters.PostAsync<ICritter, ICritter>(f => f.Name = "New critter on the block", null);
            Assert.That(result.Name, Is.EqualTo("New critter on the block"));
        }
    }
}
