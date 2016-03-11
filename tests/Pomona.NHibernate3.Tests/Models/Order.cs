#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pomona.NHibernate3.Tests.Models
{
    public class Order
    {
        public Order()
        {
            Lines = new List<OrderLine>();
        }


        public virtual int Id { get; protected set; }
        public virtual IList<OrderLine> Lines { get; protected set; }

        public virtual IEnumerable<OrderLine> LinesWithOddIds
        {
            get { return Lines.Where(x => x.Id % 2 == 1); }
        }

        public virtual string Reference { get; set; }
    }
}