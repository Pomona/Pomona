namespace Pomona.TestModel
{
    public class Bazooka : EntityBase
    {
        public Bazooka(BazookaModel model)
        {
            this.Model = model;
        }
        public BazookaModel Model { get; set; }

        public double ExplosionFactor { get; set; }
        public double Dependability { get; set; }
    }
}