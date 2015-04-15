using Pomona.Common.TypeSystem;

namespace Pomona.Documentation.Nodes
{
    public interface ISeeNode : IDocNode
    {
        MemberSpec Reference { get; }
    }
}