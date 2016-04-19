#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models.Existence
{
    public abstract class Planemo : CelestialObject
    {
        public Planemo(string name, PlanetarySystem planetarySystem)
            : base(name)
        {
            PlanetarySystem = planetarySystem;
            planetarySystem.Planets.Add(this);
        }


        public Planemo()
        {
        }


        public PlanetarySystem PlanetarySystem { get; set; }
    }
}