#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.UnitTests.PomonaSession
{
    public abstract class SessionTestsBase
    {
        public Critter FirstCritter { get; private set; }

        public int FirstCritterId => FirstCritter.Id;

        public MusicalCritter MusicalCritter => Repository.List<Critter>().OfType<MusicalCritter>().First();

        public int MusicalCritterId => MusicalCritter.Id;

        protected CritterRepository Repository { get; private set; }
        protected TypeMapper TypeMapper { get; private set; }


        [SetUp]
        public void SetUp()
        {
            TypeMapper = new TypeMapper(new CritterPomonaConfiguration());
            Repository = new CritterRepository(TypeMapper);
            FirstCritter = Repository.List<Critter>().First();
        }
    }
}
