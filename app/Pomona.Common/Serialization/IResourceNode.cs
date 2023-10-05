#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface IResourceNode
    {
        IResourceNode Parent { get; }
        TypeSpec ResultType { get; }
        object Value { get; }
    }
}

