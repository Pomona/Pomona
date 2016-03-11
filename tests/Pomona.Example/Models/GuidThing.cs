#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class GuidThing
    {
        public GuidThing(Guid? guid = null)
        {
            Id = guid ?? Guid.NewGuid();
        }


        public Guid Id { get; }
    }
}