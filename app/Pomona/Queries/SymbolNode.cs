#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Queries
{
    internal class SymbolNode : NodeBase
    {
        public SymbolNode(string name, IEnumerable<NodeBase> children)
            : this(NodeType.Symbol, name, children)
        {
        }


        protected SymbolNode(NodeType nodeType, string name, IEnumerable<NodeBase> children)
            : base(nodeType, children)
        {
            if (!string.IsNullOrEmpty(name) && name[0] == '@')
                name = name.Substring(1);

            if (name != null)
                Name = name;
        }


        public bool HasArguments
        {
            get { return Children.Count > 0; }
        }

        public string Name { get; }


        public override string ToString()
        {
            if (Children.Count == 0)
                return String.Format("{0}", Name);
            else
            {
                return String.Format(
                    "{0}({1})", Name,
                    string.Join(", ", Children.Select(x => x.ToString())));
            }
        }
    }
}