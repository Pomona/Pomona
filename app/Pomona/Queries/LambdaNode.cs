#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Queries
{
    internal class LambdaNode : NodeBase
    {
        public LambdaNode(IEnumerable<NodeBase> children)
            : base(NodeType.Lambda, children)
        {
            if (Children.Count != 2)
                throw new PomonaExpressionSyntaxException("Error parsing lambda expression");

            if (Children[0].NodeType != NodeType.Symbol)
                throw new PomonaExpressionSyntaxException("Left side of lambda expression needs to be a symbol.");

            Argument = Children[0] as SymbolNode;
            Body = Children[1];
        }


        public SymbolNode Argument { get; }

        public NodeBase Body { get; }
    }
}
