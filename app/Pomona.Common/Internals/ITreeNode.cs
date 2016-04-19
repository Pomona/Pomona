#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Common.Internals
{
    public interface ITreeNode<out T>
        where T : class, ITreeNode<T>
    {
        IEnumerable<T> Children { get; }
        T Parent { get; }
    }
}