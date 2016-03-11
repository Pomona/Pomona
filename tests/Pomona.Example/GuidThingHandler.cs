#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class GuidThingHandler
    {
        public GuidThing Get(Guid guid)
        {
            return new GuidThing(guid);
        }


        public GuidThing Post(GuidThing guidThing)
        {
            return guidThing;
        }
    }
}