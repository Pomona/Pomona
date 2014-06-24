#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class CollectionDelta<TElement, TCollection> : CollectionDelta<TElement>, IDelta<TCollection>
    {
        public CollectionDelta(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        public new TCollection Original
        {
            get { return (TCollection)base.Original; }
        }
    }

    public class CollectionDelta : Delta, ICollectionDelta
    {
        private static readonly MethodInfo addOriginalItem =
            ReflectionHelper.GetMethodDefinition<CollectionDelta<object, IEnumerable>>(
                x => x.AddOriginalItem<object>(null));

        private static readonly MethodInfo removeOriginalItem =
            ReflectionHelper.GetMethodDefinition<CollectionDelta<object, IEnumerable>>(
                x => x.RemoveOriginalItem<object>(null));

        private readonly HashSet<object> added = new HashSet<object>();
        private readonly Dictionary<object, Delta> nestedDeltaMap = new Dictionary<object, Delta>();
        private readonly HashSet<object> removed = new HashSet<object>();
        private readonly HashSet<Delta> tracked = new HashSet<Delta>();
        private bool cleared;
        private bool originalIsLoaded = false;


        public CollectionDelta(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected CollectionDelta()
        {
        }


        public IEnumerable<object> AddedItems
        {
            get { return this.added.Select(x => x is Delta ? ((Delta)x).Original : x); }
        }

        public bool Cleared
        {
            get { return this.cleared; }
        }

        public int Count
        {
            get { return OriginalCount + this.added.Count - this.removed.Count; }
        }

        public IEnumerable<Delta> ModifiedItems
        {
            get { return this.tracked.Where(x => x.IsDirty); }
        }

        public int OriginalCount
        {
            get { return TrackedItems.Count; }
        }

        public bool OriginalItemsLoaded
        {
            get { return false; }
        }

        public IEnumerable<object> RemovedItems
        {
            get { return this.removed.Select(x => x is Delta ? ((Delta)x).Original : x); }
        }

        protected ISet<Delta> TrackedItems
        {
            get
            {
                if (!this.originalIsLoaded)
                {
                    CreateItemsWrapper().AddTo(this.tracked);
                    this.originalIsLoaded = true;
                }
                return this.tracked;
            }
        }


        public override void Apply()
        {
            var nonGenericList = Original as IList;

            // Important to cache these, if not a "collection modified during enumeration" exception will be thrown.
            var removedItems = RemovedItems.ToList();
            var addedItems = AddedItems.ToList();
            var modifiedItems = ModifiedItems.ToList();
            foreach (var item in removedItems)
            {
                if (nonGenericList != null)
                    nonGenericList.Remove(item);
                else
                {
                    removeOriginalItem.MakeGenericMethod(Type.ElementType.Type)
                        .Invoke(this, new[] { item });
                }
            }
            foreach (var item in addedItems)
            {
                if (nonGenericList != null)
                    nonGenericList.Add(item);
                else
                {
                    addOriginalItem.MakeGenericMethod(Type.ElementType.Type)
                        .Invoke(this, new[] { item });
                }
            }
            foreach (var item in modifiedItems)
                item.Apply();
            if (Parent == null)
                Reset();
        }


        public override void Reset()
        {
            if (!OriginalItemsLoaded)
            {
                // No nested deltas possible when original items has not been loaded
                this.added.Clear();
                this.tracked.Clear();
                this.removed.Clear();
                this.nestedDeltaMap.Clear();
                this.cleared = false;
            }
            else
                throw new NotImplementedException();
            base.Reset();
        }


        public void AddItem(object item)
        {
            // OK: Transient item added, has not been added before.
            // ??: Transient item added, has already been added
            // OK: Persisted item added, was previously removed
            // ??: Persistem item added, that is already part of collection
            // ??: Dirty (Delta) item previously removed added
            // ??: Dirty (Delta) item added, that has not previously been removed
            // ??: Clean (Delta) item previously removed added
            // ??: Clean (Delta) item added, that has not previously been removed

            // If item has previously been marked for removal it must be a new object, thus added.

            if (IsPersistedItem(item))
            {
                item = GetWrappedItem(item);
                if (!this.removed.Remove(item))
                    added.Add(item);
                SetDirty();
            }
            else
            {
                this.added.Add(item);
                SetDirty();
            }
        }


        public void Clear()
        {
            this.cleared = true;
            SetDirty();
        }


        public bool RemoveItem(object item)
        {
            // OK: Transient item removed, was previously added
            // OK: Persisted item removed
            // ??: Dirty (Delta) item removed.
            // If item has been added to patch, do not put it in list for pending removals.

            // Persisted (in original collection)
            if (IsPersistedItem(item))
            {
                item = GetWrappedItem(item);
                if (this.cleared || this.removed.Contains(item))
                    return false;
                if (!this.added.Remove(item))
                    this.removed.Add(item);
                SetDirty();
                return true;
            }

            // Transient (not in original collection)
            return this.added.Remove(item);
        }


        protected bool IsPersistedItem(object item)
        {
            var delta = item as Delta;
            if (delta != null)
            {
                if (delta.Parent != this)
                    throw new InvalidOperationException("Nested delta is not owned by this collection.");
                return true;
            }
            var resource = item as IClientResource;
            if (resource != null)
                return resource.IsPersisted();
            return false;
        }


        internal static object CreateTypedCollectionDelta(object original,
            TypeSpec type,
            ITypeMapper typeMapper,
            Delta parent,
            Type propertyType)
        {
            var collectionType = typeof(CollectionDelta<,>).MakeGenericType(type.ElementType.Type,
                type.Type);
            if (!propertyType.IsAssignableFrom(collectionType))
            {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else
                // Need to create a runtime proxy for custom collection (repository).
                var repoProxyBaseType = typeof(RepositoryDeltaProxyBase<,>).MakeGenericType(type.ElementType, type.Type);
                var proxy = (CollectionDelta)RuntimeProxyFactory.Create(repoProxyBaseType, type.Type);
                proxy.Original = new object[] { }; // Don't want to track original items of child repositories.
                proxy.Type = type;
                proxy.TypeMapper = typeMapper;
                proxy.Parent = parent;
                return proxy;
#endif
            }
            return Activator.CreateInstance(collectionType, original, type, typeMapper, parent);
        }


        private void AddOriginalItem<T>(object item)
        {
            ((ICollection<T>)Original).Add((T)item);
        }


        private IEnumerable<Delta> CreateItemsWrapper()
        {
            foreach (var origItem in (IEnumerable)Original)
            {
                if (origItem == null)
                    yield return null;
                else
                    yield return GetWrappedItem(origItem);
            }
        }


        private Delta GetWrappedItem(object item)
        {
            var delta = item as Delta;
            if (delta != null)
                return delta;

            return this.nestedDeltaMap.GetOrCreate(item,
                () =>
                {
                    var origItemType = TypeMapper.GetClassMapping(item.GetType());
                    if (origItemType.SerializationMode == TypeSerializationMode.Complex)
                        return (Delta)CreateNestedDelta(item, origItemType, Type.ElementType);
                    throw new InvalidOperationException("Unable to wrap non-complex type in nested delta.");
                });
        }


        private void RemoveOriginalItem<T>(object item)
        {
            ((ICollection<T>)Original).Remove((T)item);
        }
    }

    public class CollectionDelta<TElement> : CollectionDelta, IList<TElement>
    {
        public CollectionDelta(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected CollectionDelta()
        {
        }


        public TElement this[int index]
        {
            get { return this.Skip(index).First(); }
            set { throw new NotImplementedException(); }
        }

        public new IEnumerable<TElement> AddedItems
        {
            get { return base.AddedItems.Cast<TElement>(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public new IEnumerable<TElement> ModifiedItems
        {
            get { return base.ModifiedItems.Cast<TElement>(); }
        }

        public new IEnumerable<TElement> RemovedItems
        {
            get { return base.RemovedItems.Cast<TElement>(); }
        }


        public void Add(TElement item)
        {
            AddItem(item);
        }


        public bool Contains(TElement item)
        {
            if (IsPersistedItem(item))
                return !RemovedItems.Contains(item);
            return AddedItems.Contains(item);
        }


        public void CopyTo(TElement[] array, int arrayIndex)
        {
            this.ToList().CopyTo(array, arrayIndex);
        }


        public IEnumerator<TElement> GetEnumerator()
        {
            return base.AddedItems.Concat(base.TrackedItems.Except(base.RemovedItems)).Cast<TElement>().GetEnumerator();
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


        public bool Remove(TElement item)
        {
            return RemoveItem(item);
        }


        public void RemoveAt(int index)
        {
            var item = this.Skip(index).FirstOrDefault();
            if (item != null)
                RemoveItem(item);
        }


        protected override object CreateNestedDelta(object propValue, TypeSpec propValueType, Type propertyTye)
        {
            return ObjectDeltaProxyBase.CreateDeltaProxy(propValue, propValueType, TypeMapper, this, propertyTye);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}