using System;

namespace Pomona.Documentation.Nodes
{
    public interface IDocNode
    {
        string ToString(Func<IDocNode, string> formattingHook);
    }
}