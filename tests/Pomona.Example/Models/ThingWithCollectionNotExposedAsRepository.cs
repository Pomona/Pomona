#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class ThingWithCollectionNotExposedAsRepository : EntityBase
    {
        public IList<Hat> Hats { get; } = new List<Hat>();
    }
}