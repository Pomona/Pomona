#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Documentation.Nodes
{
    internal class UnresolvedSeeNode : DocNode
    {
        private readonly string name;


        public UnresolvedSeeNode(string name)
        {
            if (name == null)
                throw new ArgumentNullException("memberSpec");
            this.name = name;
        }


        protected override string OnToString(Func<IDocNode, string> formattingHook)
        {
            return this.name;
        }
    }
}