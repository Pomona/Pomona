#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class Loner : EntityBase
    {
        public Loner(string name, int strength, string optionalInfo, DateTime? optionalDate = null)
        {
            Name = name;
            Strength = strength;
            OptionalInfo = optionalInfo;
            OptionalDate = optionalDate ?? DateTime.UtcNow;
            Occupation = "default boring";
        }


        public string Name { get; }

        public string Occupation { get; set; }

        public DateTime OptionalDate { get; }

        public string OptionalInfo { get; }

        public int Strength { get; }
    }
}
