#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.Internals
{
    public class MappedReadOnlyCollection<TOuter, TInner> : ICollection<TOuter>
    {
        public MappedReadOnlyCollection(ICollection<TInner> inner, Func<TInner, TOuter> map)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (map == null)
                throw new ArgumentNullException(nameof(map));
            Inner = inner;
            Map = map;
        }


        protected ICollection<TInner> Inner { get; }

        protected Func<TInner, TOuter> Map { get; }


        public void Add(TOuter item)
        {
            throw new NotSupportedException("Collection is read-only");
        }


        public void Clear()
        {
            throw new NotSupportedException("Collection is read-only");
        }


        public bool Contains(TOuter item)
        {
            return Inner.Select(Map).Contains(item);
        }


        public void CopyTo(TOuter[] array, int arrayIndex)
        {
            Inner.Select(Map).ToList().CopyTo(array, arrayIndex);
        }


        public int Count => Inner.Count;


        public IEnumerator<TOuter> GetEnumerator()
        {
            return Inner.Select(Map).GetEnumerator();
        }


        public bool IsReadOnly => true;


        public bool Remove(TOuter item)
        {
            throw new NotSupportedException("Collection is read-only");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

