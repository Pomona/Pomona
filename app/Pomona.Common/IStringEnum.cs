#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common
{
    public interface IStringEnum<T> : IStringEnum
        where T : struct
    {
    }

    public interface IStringEnum
    {
    }
}