#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.TypeSystem
{
    public class SubclassComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            if (x == y)
                return 0;

            return x.IsAssignableFrom(y)
                ? -1
                : 1;
        }
    }
}

