#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

namespace Pomona.Queries
{
    internal abstract class LiteralNode<T> : NodeBase
    {
        protected LiteralNode(NodeType nodeType, T value)
            : base(nodeType, Enumerable.Empty<NodeBase>())
        {
            Value = value;
        }


        public T Value { get; }
    }
}