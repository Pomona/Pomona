using System.Collections.Generic;

namespace PomonaNHibernateTest.Models
{
    public class EntityAttribute
    {
        public virtual int Id { get; set; }
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }
    }

    public class EntityBase
    {
        private IList<EntityAttribute> attributes;


        public EntityBase()
        {
            this.attributes = new List<EntityAttribute>();
        }


        public virtual IList<EntityAttribute> Attributes
        {
            get { return this.attributes; }
            set { this.attributes = value; }
        }

        public virtual int Id { get; set; }
    }

    public class Product : EntityBase
    {
        public virtual string Name { get; set; }
        public virtual decimal Price { get; set; }
        public virtual string Sku { get; set; }
    }

    public class Item : EntityBase
    {
        public virtual PurchaseOrder Order { get; set; }
        public virtual decimal Price { get; set; }
        public virtual Product Product { get; set; }
        public virtual int Quantity { get; set; }
    }

    public class PurchaseOrder : EntityBase
    {
        private IList<Item> items;


        public PurchaseOrder()
        {
            this.items = new List<Item>();
        }


        public virtual Customer Customer { get; set; }

        public virtual IList<Item> Items
        {
            get { return this.items; }
            set { this.items = value; }
        }

        public virtual int SomeGroup { get; set; }
    }

    public class Customer : EntityBase
    {
        public virtual string Name { get; set; }
    }
}