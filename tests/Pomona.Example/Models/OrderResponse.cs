#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public class OrderResponse
    {
        public OrderResponse(Order order)
        {
            Order = order;
        }


        public Order Order { get; }
    }
}

