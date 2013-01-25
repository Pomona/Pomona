namespace PomonaNHibernateTest.Models
{
    public class Item : EntityBase
    {
        public virtual PurchaseOrder Order { get; set; }
        public virtual decimal Price { get; set; }
        public virtual Product Product { get; set; }
        public virtual int Quantity { get; set; }
    }
}