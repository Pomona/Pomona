#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public class MusicalCritter : Critter
    {
        public MusicalCritter(string onlyWritableByInheritedResource)
        {
            OnlyWritableByInheritedResource = onlyWritableByInheritedResource;
        }


        public string BandName { get; set; }
        public Instrument Instrument { get; set; }
    }
}

