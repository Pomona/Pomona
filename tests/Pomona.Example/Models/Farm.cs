#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pomona.Example.Models
{
    public class Farm : EntityBase
    {
        public Farm(string name)
        {
            Name = name;
            Critters = new List<Critter>();
        }


        public List<Critter> Critters { get; set; }

        public IEnumerable<MusicalCritter> MusicalCritters
        {
            get { return Critters.OfType<MusicalCritter>(); }
        }

        public string Name { get; set; }
    }
}