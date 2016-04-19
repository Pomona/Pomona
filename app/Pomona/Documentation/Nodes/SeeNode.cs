#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Documentation.Nodes
{
    internal class SeeNode : DocNode, ISeeNode
    {
        public SeeNode(MemberSpec reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));
            Reference = reference;
        }


        protected override string OnToString(Func<IDocNode, string> formattingHook)
        {
            return "(ref: " + Reference.Name + ")";
        }


        public MemberSpec Reference { get; private set; }
    }
}