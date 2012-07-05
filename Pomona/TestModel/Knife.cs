namespace Pomona.TestModel
{
    public class Knife : Weapon
    {
        public Knife(WeaponModel model) : base(model)
        {
        }

        public double Sharpness { get; set; }
    }
}