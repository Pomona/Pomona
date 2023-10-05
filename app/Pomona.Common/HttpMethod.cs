#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Common
{
    [Flags]
    public enum HttpMethod
    {
        /// <summary>
        /// Property is readable.
        /// </summary>
        Get = 1,

        Post = 1 << 1,

        /// <summary>
        /// Property is settable, always implies that Post is also allowed.
        /// </summary>
        Put = 1 << 2,

        Patch = 1 << 3,

        Delete = 1 << 4
    }
}

