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