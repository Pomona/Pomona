#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.NHibernate3.Tests.Models
{
    public class OrderLine
    {
        private Order order;


        public OrderLine(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            this.order = order;
        }


        protected OrderLine()
        {
        }


        public virtual string Description { get; set; }
        public virtual int Id { get; protected set; }

        public virtual Order Order
        {
            get { return this.order; }
            protected set { this.order = value; }
        }
    }
}