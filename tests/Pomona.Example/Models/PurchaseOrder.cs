#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class PurchaseOrder : Order
    {
        public PurchaseOrder(IEnumerable<OrderItem> items)
            : base(items)
        {
        }


        public string ThePurchaseInfo { get; set; }
    }
}