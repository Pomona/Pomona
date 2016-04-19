#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.TypeSystem
{
    [Flags]
    public enum PropertyFlags
    {
        IsReadable = 1,
        IsWritable = 2,
        AllowsFiltering = 4
    }
}