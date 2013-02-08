using System.Collections.Generic;

namespace Pomona.Queries
{
    internal class ArrayNode : NodeBase
    {
        public ArrayNode(IEnumerable<NodeBase> children) : base(NodeType.ArrayLiteral, children)
        {
        }
    }
}