#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models.Existence
{
    public class Planet : Planemo
    {
        public Planet()
        {
        }


        public Planet(string name, PlanetarySystem planetarySystem)
            : base(name, planetarySystem)
        {
        }


        public ICollection<Moon> Moons { get; } = new List<Moon>();
    }
}

