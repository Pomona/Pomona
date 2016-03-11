#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.Serialization.Patch
{
    public interface IDelta<T> : IDelta
    {
        new T Original { get; }
    }

    public interface IDelta
    {
        bool IsDirty { get; }
        object Original { get; }
        void Apply();
    }
}