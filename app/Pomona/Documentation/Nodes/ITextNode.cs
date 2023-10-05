#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Documentation.Nodes
{
    public interface ITextNode : IDocNode
    {
        string Value { get; }
    }
}

