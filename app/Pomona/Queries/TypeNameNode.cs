#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Queries
{
    internal class TypeNameNode : LiteralNode<string>
    {
        public TypeNameNode(string typeName)
            : base(NodeType.TypeNameLiteral, typeName)
        {
        }
    }
}