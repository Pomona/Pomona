namespace Pomona.TestModel
{
    public class Gun : Weapon
    {
        public Gun(WeaponModel model) : base(model)
        {
        }

        public double ExplosionFactor { get; set; }
    }
}