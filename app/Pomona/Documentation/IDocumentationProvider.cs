#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common.TypeSystem;
using Pomona.Documentation.Nodes;

namespace Pomona.Documentation
{
    public interface IDocumentationProvider
    {
        IDocNode GetSummary(MemberSpec member);
    }
}
