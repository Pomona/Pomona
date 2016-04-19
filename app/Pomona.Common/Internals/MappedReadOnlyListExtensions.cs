#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.Internals
{
    public static class MappedReadOnlyListExtensions
    {
        public static IList<TOuter> MapList<TOuter, TInner>(this IList<TInner> inner, Func<TInner, TOuter> mapFunction)
        {
            return new MappedReadOnlyList<TOuter, TInner>(inner, mapFunction);
        }
    }
}