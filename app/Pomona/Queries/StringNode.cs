#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

namespace Pomona.Queries
{
    internal class StringNode : NodeBase
    {
        public StringNode(string unescapedString)
            : base(NodeType.StringLiteral, Enumerable.Empty<NodeBase>())
        {
            Value = UnescapeText(unescapedString);
        }


        public string Value { get; }


        public override string ToString()
        {
            return $"{base.ToString()} '{Value}'";
        }


        private string UnescapeText(string unescapedString)
        {
            if (unescapedString[0] != '\'' || unescapedString[unescapedString.Length - 1] != '\'')
                throw new InvalidOperationException("Don't know how to unescape string, expected quotes around");

            // TODO: Proper unescaping of strings
            return unescapedString.Substring(1, unescapedString.Length - 2).Replace("''", "'");
        }
    }
}