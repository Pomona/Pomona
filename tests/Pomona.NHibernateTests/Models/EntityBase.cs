using System.Collections.Generic;

namespace PomonaNHibernateTest.Models
{
    public class EntityBase
    {
        private IList<EntityAttribute> attributes;


        public EntityBase()
        {
            attributes = new List<EntityAttribute>();
        }


        public virtual IList<EntityAttribute> Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public virtual int Id { get; set; }
    }
}