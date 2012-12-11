using System.Collections.Generic;

namespace PomonaNHibernateTest.Models
{
    public class PurchaseOrder : EntityBase
    {
        private IList<Item> items;


        public PurchaseOrder()
        {
            items = new List<Item>();
        }


        public virtual Customer Customer { get; set; }

        public virtual IList<Item> Items
        {
            get { return items; }
            set { items = value; }
        }

        public virtual int SomeGroup { get; set; }
    }
}