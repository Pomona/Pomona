#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Common.Internals
{
    public interface ITreeNodeWithRoot<out T> : ITreeNode<T>
        where T : class, ITreeNode<T>
    {
        T Root { get; }
    }
}

