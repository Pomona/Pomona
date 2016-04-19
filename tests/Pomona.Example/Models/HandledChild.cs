#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class HandledChild : EntityBase
    {
        public HandledChild(HandledThing parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            Parent = parent;
            Parent.Children.Add(this);
        }


        public bool HandlerWasCalled { get; set; }

        public HandledThing Parent { get; }

        public string Toy { get; set; }
    }
}