#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
        public CollectionDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
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

        private static readonly MethodInfo createTypedCollectionDeltaMethod =
            ReflectionHelper.GetMethodDefinition(() => CreateTypedCollectionDelta<object, object>(null, null, null, null, null));

        private readonly HashSet<object> added = new HashSet<object>();
        private readonly Dictionary<object, Delta> nestedDeltaMap = new Dictionary<object, Delta>();
        private readonly HashSet<object> removed = new HashSet<object>();
        private readonly HashSet<Delta> tracked = new HashSet<Delta>();
        private bool originalIsLoaded = false;


        public CollectionDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected CollectionDelta()
        {
        }


        public int Count
        {
            get { return OriginalCount + this.added.Count - this.removed.Count; }
        }

        public int OriginalCount
        {
            get { return TrackedItems.Count; }
        }

        public bool OriginalItemsLoaded
        {
            get { return false; }
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


        public bool AddItem(object item)
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

            bool added = false;
            if (IsPersistedItem(item))
            {
                item = GetWrappedItem(item);
                if (!this.removed.Remove(item))
                    added = this.added.Add(item);
                SetDirty();
            }
            else
            {
                added = this.added.Add(item);
                if (added)
                    SetDirty();
            }
            return added;
        }


        public void Clear()
        {
            Cleared = true;
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
                if (Cleared || this.removed.Contains(item))
                    return false;
                if (!this.added.Remove(item))
                    this.removed.Add(item);
                SetDirty();
                return true;
            }

            // Transient (not in original collection)
            return this.added.Remove(item);
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
                Cleared = false;
            }
            else
                throw new NotImplementedException();
            base.Reset();
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
                                                          ITypeResolver typeMapper,
                                                          Delta parent,
                                                          Type propertyType)
        {
            return createTypedCollectionDeltaMethod.MakeGenericMethod(type.ElementType, type)
                                                   .Invoke(null, new object[] { original, type, typeMapper, parent, propertyType });
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


        private static object CreateTypedCollectionDelta<TElement, TCollection>(object original,
                                                                                TypeSpec type,
                                                                                ITypeResolver typeMapper,
                                                                                Delta parent,
                                                                                Type propertyType)
        {
            if (propertyType.IsAssignableFrom(typeof(ICollection<TElement>)))
                return new CollectionDelta<TElement, TCollection>(original, type, typeMapper, parent);
            if (propertyType.IsAssignableFrom(typeof(IList<TElement>)))
                return new ListDelta<TElement, TCollection>(original, type, typeMapper, parent);
            if (propertyType.IsAssignableFrom(typeof(ISet<TElement>)))
                return new SetDelta<TElement, TCollection>(original, type, typeMapper, parent);

#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else
            // Need to create a runtime proxy for custom collection (repository).
            var repoProxyBaseType = typeof(RepositoryDeltaProxyBase<TElement, TCollection>);
            var proxy = (CollectionDelta)RuntimeProxyFactory.Create(repoProxyBaseType, type.Type);
            proxy.Original = new object[] { }; // Don't want to track original items of child repositories.
            proxy.Type = type;
            proxy.TypeMapper = typeMapper;
            proxy.Parent = parent;
            return proxy;
#endif
        }


        private Delta GetWrappedItem(object item)
        {
            var delta = item as Delta;
            if (delta != null)
                return delta;

            return this.nestedDeltaMap.GetOrCreate(item,
                                                   () =>
                                                   {
                                                       var origItemType = TypeMapper.FromType(item.GetType());
                                                       if (origItemType.SerializationMode == TypeSerializationMode.Structured)
                                                           return (Delta)CreateNestedDelta(item, origItemType, Type.ElementType);
                                                       throw new InvalidOperationException(
                                                           "Unable to wrap non-complex type in nested delta.");
                                                   });
        }


        private void RemoveOriginalItem<T>(object item)
        {
            ((ICollection<T>)Original).Remove((T)item);
        }


        public IEnumerable<object> AddedItems
        {
            get { return this.added.Select(x => x is Delta ? ((Delta)x).Original : x); }
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


        public bool Cleared { get; private set; }

        public IEnumerable<Delta> ModifiedItems
        {
            get { return this.tracked.Where(x => x.IsDirty); }
        }

        public IEnumerable<object> RemovedItems
        {
            get { return this.removed.Select(x => x is Delta ? ((Delta)x).Original : x); }
        }
    }

    public class CollectionDelta<TElement> : CollectionDelta, ICollection<TElement>
    {
        public CollectionDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        protected CollectionDelta()
        {
        }


        public new IEnumerable<TElement> AddedItems
        {
            get { return base.AddedItems.Cast<TElement>(); }
        }

        public new IEnumerable<TElement> ModifiedItems
        {
            get { return base.ModifiedItems.Cast<TElement>(); }
        }

        public new IEnumerable<TElement> RemovedItems
        {
            get { return base.RemovedItems.Cast<TElement>(); }
        }


        protected override object CreateNestedDelta(object propValue, TypeSpec propValueType, Type propertyTye)
        {
            return ObjectDeltaProxyBase.CreateDeltaProxy(propValue, propValueType, TypeMapper, this, propertyTye);
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
            return base.AddedItems.Concat(TrackedItems.Except(base.RemovedItems)).Cast<TElement>().GetEnumerator();
        }


        public bool IsReadOnly
        {
            get { return false; }
        }


        public bool Remove(TElement item)
        {
            return RemoveItem(item);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}