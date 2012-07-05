using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CritterClient;

using NUnit.Framework;

using Pomona.Client;

namespace CritterClientTests
{
    [TestFixture]
    public class CritterTests
    {
        [Test]
        public void DeserializeCritters()
        {
            var client = new ClientHelper();

            var critters = client.List<Critter>("critter.weapons.model,critter.subscriptions");

        }
    }
}
