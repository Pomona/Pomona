#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Queries
{
    internal class ArrayNode : NodeBase
    {
        public ArrayNode(IEnumerable<NodeBase> children)
            : base(NodeType.ArrayLiteral, children)
        {
        }
    }
}