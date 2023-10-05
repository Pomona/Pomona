#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    public class NodeBase
    {
        public NodeBase(NodeType nodeType, IEnumerable<NodeBase> children)
        {
            NodeType = nodeType;
            Children = new List<NodeBase>(children);
        }


        public List<NodeBase> Children { get; }

        public NodeType NodeType { get; }

        internal ITree ParserNode { get; set; }


        public override string ToString()
        {
            return NodeType.ToString();
        }
    }
}
