using System.Collections.Generic;

namespace Pomona.Documentation.Nodes
{
    public interface IDocContainerNode : IDocNode
    {
        ICollection<IDocNode> Children { get; }
    }
}