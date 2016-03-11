#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class HandledThing : EntityBase, ISetEtaggedEntity
    {
        private readonly HashSet<HandledChild> children = new HashSet<HandledChild>();


        public HandledThing()
        {
            SingleChild = new HandledSingleChild(this) { Name = "The loner" };
        }


        public ISet<HandledChild> Children
        {
            get { return this.children; }
        }

        public string ETag { get; private set; } = Guid.NewGuid().ToString();

        public int FetchedCounter { get; set; }
        public string Foo { get; set; }
        public string Marker { get; set; }
        public int PatchCounter { get; set; }
        public int QueryCounter { get; set; }
        public HandledSingleChild SingleChild { get; set; }


        public void SetEtag(string newEtagValue)
        {
            ETag = newEtagValue;
        }
    }
}