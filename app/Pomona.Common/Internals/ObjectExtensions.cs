#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Common.Internals
{
    public static class ObjectExtensions
    {
        public static T[] WrapAsArray<T>(this T value)
        {
            return new T[] { value };
        }


        public static IEnumerable<T> WrapAsEnumerable<T>(this T value)
        {
            yield return value;
        }
    }
}

