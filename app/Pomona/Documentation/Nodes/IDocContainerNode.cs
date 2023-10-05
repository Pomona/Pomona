#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Documentation.Nodes
{
    public interface IDocContainerNode : IDocNode
    {
        ICollection<IDocNode> Children { get; }
    }
}
