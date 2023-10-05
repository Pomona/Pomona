#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models.Existence
{
    public class PlanetarySystem : CelestialObject
    {
        public PlanetarySystem()
        {
            Star = new Star() { Name = "Sun", PlanetarySystem = this };
        }


        public Galaxy Galaxy { get; set; }
        // Well technically a planetary system can have multiple stars, but lets ignore that to use it as example
        // of a single child resource.

        public ICollection<Planemo> Planets { get; } = new List<Planemo>();

        public Star Star { get; set; }
    }
}

