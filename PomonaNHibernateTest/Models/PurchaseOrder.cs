using System.Collections.Generic;

namespace PomonaNHibernateTest.Models
{
    public class EntityBase
    {
        public virtual int Id { get; set; }
    }

    public class Product : EntityBase
    {
        public virtual decimal Price { get; set; }
        public virtual string Sku { get; set; }
        public virtual string Name { get; set; }
    }

    public class Item : EntityBase
    {
        public virtual PurchaseOrder Order { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal Price { get; set; }
        public virtual Product Product { get; set; }
    }

    public class PurchaseOrder : EntityBase
    {
        public virtual int SomeGroup { get; set; }
        private IList<Item> _items;

        public PurchaseOrder()
        {
            _items = new List<Item>();
        }

        public virtual Customer Customer { get; set; }
        public virtual IList<Item> Items
        {
            get { return _items; }
            set { _items = value; }
        }
    }

    public class Customer : EntityBase
    {
        public virtual string Name { get; set; }
    }
}