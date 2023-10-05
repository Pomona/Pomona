#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public class Weapon : EntityBase
    {
        public Weapon(WeaponModel model)
        {
            Model = model;
        }


        public WeaponModel Model { get; set; }
        public decimal Price { get; set; }
        public double Strength { get; set; }
    }
}

