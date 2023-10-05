#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Proxies;

namespace Pomona.UnitTests.Client
{
    [TestFixture]
    public class ClientResourceTests
    {
        [Test]
        public void IsPersisted_OnPostForm_ReturnsFalse()
        {
            Assert.That((new DummyForm()).IsPersisted(), Is.False);
        }


        [Test]
        public void IsPersisted_OnResourceForm_ReturnsFalse()
        {
            Assert.That((new DummyResource()).IsPersisted(), Is.True);
        }


        [Test]
        public void IsTransient_OnPostForm_ReturnsTrue()
        {
            Assert.That((new DummyForm()).IsTransient(), Is.True);
        }


        [Test]
        public void IsTransient_OnResourceForm_ReturnsFalse()
        {
            Assert.That((new DummyResource()).IsTransient(), Is.False);
        }


        public class DummyForm : PostResourceBase, IClientResource
        {
        }

        public class DummyResource : ResourceBase, IClientResource
        {
        }
    }
}

