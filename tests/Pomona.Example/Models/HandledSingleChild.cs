#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models
{
    public class HandledSingleChild : EntityBase
    {
        public HandledSingleChild(HandledThing handledThing)
        {
            HandledThing = handledThing;
        }


        public HandledThing HandledThing { get; private set; }
        public string Name { get; set; }
        public bool PatchHandlerCalled { get; set; }
    }
}