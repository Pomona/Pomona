#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.NHibernate3.Tests.Models
{
    public class Customer
    {
        private string name;


        public Customer(string name)
        {
            this.name = name;
        }


        protected Customer()
        {
        }


        public virtual int Id { get; set; }

        public virtual string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}