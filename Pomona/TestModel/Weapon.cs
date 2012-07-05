namespace Pomona.TestModel
{
    public class Weapon : EntityBase
    {
        public Weapon(WeaponModel model)
        {
            this.Model = model;
        }
        public WeaponModel Model { get; set; }

        public double Dependability { get; set; }
    }
}