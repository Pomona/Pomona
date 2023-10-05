#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Critters.Client;
using Critters.Client.Pomona.Common;

using NUnit.Framework;

using Pomona.Example.Models;

namespace Pomona.IndependentClientTests
{
    [TestFixture]
    public class QueryTests : IndependentClientTestsBase
    {
        [Test]
        public void SimpleQueryTest()
        {
            var musicalCritterId = CritterEntities.OfType<MusicalCritter>().First().Id;

            var musicalCritter = ResourceFetcherExtensions.Get<ICritter>(Client, BaseUri + "critters/" + musicalCritterId);

            Assert.That(musicalCritter, Is.AssignableTo<IMusicalCritter>());
        }
    }
}

