#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class ArgNullThrowingThing : EntityBase
    {
        public ArgNullThrowingThing(string incoming)
        {
            if (incoming == null)
                throw new ArgumentNullException(nameof(incoming));
            Incoming = incoming;
        }


        public string Incoming { get; }
    }
}