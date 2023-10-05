#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.Internals
{
    public class MappedReadOnlyList<TOuter, TInner> : MappedReadOnlyCollection<TOuter, TInner>, IList<TOuter>
    {
        public MappedReadOnlyList(IList<TInner> inner, Func<TInner, TOuter> map)
            : base(inner, map)
        {
        }


        public int IndexOf(TOuter item)
        {
            return
                Inner.Select((x, i) => new { x, i }).Where(y => EqualityComparer<TOuter>.Default.Equals(Map(y.x), item))
                     .Select(y => (int?)y.i).FirstOrDefault() ?? -1;
        }


        public void Insert(int index, TOuter item)
        {
            throw new NotSupportedException("Collection is read-only");
        }


        public TOuter this[int index]
        {
            get { return Map(((IList<TInner>)Inner)[index]); }
            set { throw new NotSupportedException("Collection is read-only"); }
        }


        public void RemoveAt(int index)
        {
            throw new NotSupportedException("Collection is read-only");
        }
    }
}

