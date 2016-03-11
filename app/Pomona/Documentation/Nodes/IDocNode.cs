#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Documentation.Nodes
{
    public interface IDocNode
    {
        string ToString(Func<IDocNode, string> formattingHook);
    }
}