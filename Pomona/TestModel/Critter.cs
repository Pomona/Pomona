using System;
using System.Collections.Generic;

namespace Pomona.TestModel
{
    public class Critter : EntityBase
    {
        public Critter()
        {
            this.Enemies = new List<Critter>();
            this.Weapons = new List<Weapon>();
            this.Subscriptions = new List<Subscription>();

            OkdayIsFun = "jada";

            Hat = new Hat();
        }


        public Hat Hat { get; set; }

        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

        public string OkdayIsFun { get; set; }

        public List<Weapon> Weapons { get; set; }
        public List<Critter> Enemies { get; set; }

        public List<Subscription> Subscriptions { get; set; }
    }
}