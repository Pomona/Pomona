#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public class Knife : Weapon
    {
        public Knife(WeaponModel model)
            : base(model)
        {
        }


        public double Sharpness { get; set; }
    }
}

