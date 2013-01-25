namespace PomonaNHibernateTest.Models
{
    public class Product : EntityBase
    {
        public virtual string Name { get; set; }
        public virtual decimal Price { get; set; }
        public virtual string Sku { get; set; }
    }
}