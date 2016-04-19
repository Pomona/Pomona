#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class Subscription : EntityBase
    {
        public Subscription(WeaponModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            Model = model;
        }


        public Critter Critter { get; set; }
        public WeaponModel Model { get; set; }
        public string Sku { get; set; }
        public DateTime StartsOn { get; set; }
    }
}