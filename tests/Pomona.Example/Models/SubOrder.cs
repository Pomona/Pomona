#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class SubOrder : Order
    {
        public SubOrder(IEnumerable<OrderItem> items)
            : base(items)
        {
        }
    }
}

