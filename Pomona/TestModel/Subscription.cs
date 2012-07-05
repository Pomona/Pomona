using System;
using System.Linq;
using System.Text;

namespace Pomona.TestModel
{
    public class Subscription : EntityBase
    {
        public Subscription(Critter critter, BazookaModel model)
        {
            if (critter == null)
                throw new ArgumentNullException("critter");
            if (model == null)
                throw new ArgumentNullException("model");
            Critter = critter;
            Model = model;
        }
        

        public Critter Critter { get; set; }

        public BazookaModel Model { get; set; }
        public string Sku { get; set; }
        public DateTime StartsOn { get; set; }
    }

}
