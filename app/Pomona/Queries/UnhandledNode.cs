#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    internal class UnhandledNode : NodeBase
    {
        public UnhandledNode(ITree sourceTreeNode)
            : base(NodeType.Unhandled, Enumerable.Empty<NodeBase>())
        {
            SourceTreeNode = sourceTreeNode;
        }


        public ITree SourceTreeNode { get; }


        public override string ToString()
        {
            return base.ToString() + " " + SourceTreeNode;
        }
    }
}
