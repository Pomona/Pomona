#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pomona.Example.Models
{
    public class Order : EntityBase
    {
        public Order(IEnumerable<OrderItem> items)
        {
            Items = items.ToList();
        }


        public string Description { get; set; }
        public IList<OrderItem> Items { get; protected set; }
    }
}

