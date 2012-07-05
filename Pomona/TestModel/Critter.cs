using System.Collections.Generic;

namespace Pomona.TestModel
{
    public class Critter : EntityBase
    {
        public Critter()
        {
            this.Enemies = new List<Critter>();
            this.Weapons = new List<Bazooka>();
            this.Subscriptions = new List<Subscription>();

            OkdayIsFun = "jada";
        }

        public string Name { get; set; }

        public string OkdayIsFun { get; set; }

        public List<Bazooka> Weapons { get; set; }
        public List<Critter> Enemies { get; set; }

        public List<Subscription> Subscriptions { get; set; }
    }
}