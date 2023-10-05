#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Critters.Client;

using NUnit.Framework;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class PostToResourceTests : ClientTestsBase
    {
        [Test]
        public void PostCaptureCommandToCritter_IsSuccessful()
        {
            var critterResource = Client.Critters.Query().First();
            critterResource = (ICritter)Client.Critters.Post(critterResource, new CritterCaptureCommandForm() { FooBar = "lalala" });
            Assert.That(critterResource.IsCaptured, Is.True);
        }


        [Test]
        public void PostExplodeCommandToCritter_IsSuccessful()
        {
            var critterResource = Client.Critters.Query().First();
            critterResource =
                (ICritter)Client.Critters.Post(critterResource, new CritterExplodeCommandForm() { Noops = "blabla" });
            Assert.That(critterResource.Name, Is.EqualTo("blabla EXPLOSION!"));
        }
    }
}

