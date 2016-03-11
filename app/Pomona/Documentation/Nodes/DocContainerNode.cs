#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Documentation.Nodes
{
    internal class DocContainerNode : DocNode, IDocContainerNode
    {
        public DocContainerNode(IEnumerable<IDocNode> children)
        {
            if (children == null)
                throw new ArgumentNullException(nameof(children));
            Children = children.ToList().AsReadOnly();
        }


        protected override string OnToString(Func<IDocNode, string> formattingHook)
        {
            return string.Concat(Children.Select(x => x.ToString(formattingHook)));
        }


        public ICollection<IDocNode> Children { get; }
    }
}