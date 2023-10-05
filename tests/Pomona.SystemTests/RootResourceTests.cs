#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using NUnit.Framework;

using Pomona.Common;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class RootResourceTests : ClientTestsBase
    {
        [Test]
        public void Get_Root_Returns_Dictionary_Containing_Resource_Links()
        {
            var dict = Client.Get<Dictionary<string, string>>("http://test/");
            Assert.That(dict, Has.Member(new KeyValuePair<string, string>("critters", "http://test/critters")));
        }
    }
}
