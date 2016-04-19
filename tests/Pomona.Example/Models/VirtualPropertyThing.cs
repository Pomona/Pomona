#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class VirtualPropertyThing : EntityBase
    {
        public VirtualPropertyThing()
        {
            Number = 100.0;
            Items = new Dictionary<string, string>();
        }


        public double Number { get; set; }
        internal Dictionary<string, string> Items { get; private set; }
    }
}