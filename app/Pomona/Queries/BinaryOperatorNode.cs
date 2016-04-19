#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Queries
{
    internal class BinaryOperatorNode : NodeBase
    {
        public BinaryOperatorNode(NodeType nodeType, IEnumerable<NodeBase> children)
            : base(nodeType, children)
        {
            if (Children.Count != 2)
                throw new ArgumentException("A binary operator always need to have 2 child nodes", nameof(children));
        }


        public NodeBase Left => Children[0];

        public NodeBase Right => Children[1];


        public override string ToString()
        {
            return $"({Left} {NodeType} {Right})";
        }
    }
}