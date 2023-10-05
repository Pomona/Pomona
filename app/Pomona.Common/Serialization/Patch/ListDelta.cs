#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class ListDelta<TElement, TList> : ListDelta<TElement>, IDelta<TList>
    {
        public ListDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        public new TList Original => (TList)base.Original;
    }

    public class ListDelta<TElement> : CollectionDelta<TElement>, IList<TElement>
    {
        public ListDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected ListDelta()
        {
        }


        public int IndexOf(TElement item)
        {
            return
                this.Select((x, i) => new { x, i }).Where(y => y.x.Equals(item)).Select(y => (int?)y.i).FirstOrDefault()
                ?? -1;
        }


        public void Insert(int index, TElement item)
        {
            AddItem(item);
        }


        public TElement this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                foreach (var item in this)
                {
                    if (index == 0)
                        return item;
                    index--;
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set { throw new NotImplementedException(); }
        }


        public void RemoveAt(int index)
        {
            var item = this.Skip(index).FirstOrDefault();
            if (item != null)
                RemoveItem(item);
        }
    }
}

