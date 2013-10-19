#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;
using Pomona.Internals;

namespace Pomona.Common.Serialization.Patch
{
    public class CollectionDelta<TElement, TCollection> : CollectionDelta<TElement>, IDelta<TCollection>
    {
        public CollectionDelta(object original, IMappedType type, ITypeMapper typeMapper, Delta parent = null) : base(original, type, typeMapper, parent)
        {
        }

        public new TCollection Original
        {
            get { return (TCollection)base.Original; }
        }
    }

    public abstract class CollectionDelta<TElement> : CollectionDelta, IList<TElement>
    {
        public CollectionDelta(object original, IMappedType type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
            if (!type.IsCollection)
                throw new ArgumentException("Original value must be collection type!");
        }

        public new IEnumerable<TElement> RemovedItems
        {
            get { return base.RemovedItems.Cast<TElement>(); }
        }

        public new IEnumerable<TElement> AddedItems
        {
            get { return base.AddedItems.Cast<TElement>(); }
        }

        public new IEnumerable<TElement> ModifiedItems
        {
            get { return base.ModifiedItems.Cast<TElement>(); }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return TrackedItems.Cast<TElement>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TElement item)
        {
            AddItem(item);
        }

        public void Clear()
        {
            TrackedItems.Clear();
            SetDirty();
        }

        public bool Contains(TElement item)
        {
            return TrackedItems.Contains(item);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            TrackedItems.Cast<TElement>().ToList().CopyTo(array, arrayIndex);
        }

        public bool Remove(TElement item)
        {
            RemoveItem(item);
            SetDirty();
            return true;
        }

        public int Count
        {
            get { return TrackedItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(TElement item)
        {
            return TrackedItems.IndexOf(item);
        }

        public void Insert(int index, TElement item)
        {
            TrackedItems.Insert(index, item);
            SetDirty();
        }

        public void RemoveAt(int index)
        {
            TrackedItems.RemoveAt(index);
            SetDirty();
        }

        public TElement this[int index]
        {
            get { return (TElement)TrackedItems[index]; }
            set
            {
                var oldItem = TrackedItems[index];
                if (oldItem == (object)value)
                    return;
                DetachFromParent(oldItem);
                TrackedItems[index] = value;
                SetDirty();
            }
        }

        protected override object CreateNestedDelta(object propValue, IMappedType propValueType)
        {
            return ObjectDeltaProxyBase.CreateDeltaProxy(propValue, propValueType, TypeMapper, this);
        }
    }

    public interface ICollectionDelta : IDelta
    {
        IEnumerable<object> RemovedItems { get; }
        IEnumerable<Delta> ModifiedItems { get; }
        IEnumerable<object> AddedItems { get; }
    }

    public interface IDelta
    {
        bool IsDirty { get; }
    }

    public interface IDelta<T> : IDelta
    {
        T Original { get; }
    }

    public class ObjectDeltaProxyBase<T> : ObjectDeltaProxyBase, IDelta<T>
        where T : class
    {
        new public T Original { get { return Original as T; } }
    }

    public class ObjectDeltaProxyBase : ObjectDelta, IPomonaSerializable
    {
        protected ObjectDeltaProxyBase()
        {
        }

        private static object Create(object original, IMappedType type, ITypeMapper typeMapper, Delta parent)
        {
            var proxy =
                RuntimeProxyFactory.Create(typeof (ObjectDeltaProxyBase<>).MakeGenericType(type.MappedTypeInstance),
                                           type.MappedTypeInstance);
            var odpb = (ObjectDeltaProxyBase)proxy;
            odpb.Original = original;
            odpb.Type = type;
            odpb.TypeMapper = typeMapper;
            odpb.Parent = parent;
            return proxy;
        }

        public static object CreateDeltaProxy(object original, IMappedType type, ITypeMapper typeMapper, Delta parent)
        {
            if (type.SerializationMode == TypeSerializationMode.Complex)
            {
                return Create(original, type, typeMapper, parent);
            }
            if (type.IsCollection)
                return CollectionDelta.CreateTypedCollectionDelta(original, type, typeMapper, parent);
            throw new NotImplementedException();
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return (TPropType)GetPropertyValue(property.Name);
        }

        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            SetPropertyValue(property.Name, value);
        }

        protected override object CreateNestedDelta(object propValue, IMappedType propValueType)
        {
            return CreateDeltaProxy(propValue, propValueType, TypeMapper, this);
        }

        public bool PropertyIsSerialized(string propertyName)
        {
            object propValue;
            return TrackedProperties.TryGetValue(propertyName, out propValue) && ValueIsDirty(propValue);
        }
    }

    public class CollectionDelta : Delta, ICollectionDelta
    {
        // Has set semantics for now, does not keep array order

        private static readonly MethodInfo addOriginalItem =
            ReflectionHelper.GetMethodDefinition<CollectionDelta<object, IEnumerable>>(x => x.AddOriginalItem<object>(null));

        private static readonly MethodInfo removeOriginalItem =
            ReflectionHelper.GetMethodDefinition<CollectionDelta<object, IEnumerable>>(x => x.RemoveOriginalItem<object>(null));

        private IList<object> trackedItems = new List<object>();
        private bool originalItemsLoaded;

        protected CollectionDelta()
        {
        }

        public CollectionDelta(object original, IMappedType type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }

        public IEnumerable<Delta> ModifiedItems
        {
            get { return TrackedItems.OfType<Delta>().Where(x => x.IsDirty); }
        }

        protected IEnumerable<object> OriginalItems
        {
            get { return ((IEnumerable)Original).Cast<object>(); }
        }

        public IEnumerable<object> AddedItems
        {
            get { return TrackedItems.Where(x => !(x is Delta)).Except(OriginalItems); }
        }

        public IEnumerable<object> RemovedItems
        {
            get { return OriginalItems.Except(TrackedOriginalItems); }
        }

        protected IEnumerable<object> TrackedOriginalItems
        {
            get
            {
                return TrackedItems.Select(x =>
                    {
                        var delta = x as Delta;
                        if (delta != null)
                            return delta.Original;
                        return x;
                    });
            }
        }

        protected IList<object> TrackedItems
        {
            get
            {
                if (!originalItemsLoaded)
                {
                    originalItemsLoaded = true;
                    foreach (var item in CreateItemsWrapper())
                    {
                        trackedItems.Add(item);
                    }
                }
                return trackedItems;
            }
        }

        internal static object CreateTypedCollectionDelta(object original, IMappedType type, ITypeMapper typeMapper,
                                                          Delta parent)
        {
            var collectionType = typeof (CollectionDelta<,>).MakeGenericType(type.ElementType.MappedTypeInstance, type.MappedTypeInstance);
            return Activator.CreateInstance(collectionType, original, type, typeMapper, parent);
        }

        public void AddItem(object item)
        {
            trackedItems.Add(item);
            SetDirty();
        }

        private IEnumerable<object> CreateItemsWrapper()
        {
            foreach (var origItem in (IEnumerable)Original)
            {
                if (origItem == null)
                {
                    yield return null;
                }
                else
                {
                    var origItemType = TypeMapper.GetClassMapping(origItem.GetType());
                    if (origItemType.SerializationMode == TypeSerializationMode.Complex)
                        yield return CreateNestedDelta(origItem, origItemType);
                }
            }
        }

        public void RemoveItem(object item)
        {
            trackedItems.Remove(item);
            DetachFromParent(item);
            SetDirty();
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
                    removeOriginalItem.MakeGenericMethod(Type.ElementType.MappedTypeInstance)
                                      .Invoke(this, new[] { item });
            }
            foreach (var item in addedItems)
            {
                if (nonGenericList != null)
                    nonGenericList.Add(item);
                else
                    addOriginalItem.MakeGenericMethod(Type.ElementType.MappedTypeInstance)
                                   .Invoke(this, new[] { item });
            }
            foreach (var item in modifiedItems)
            {
                item.Apply();
            }
            if (Parent == null)
                Reset();
        }

        private void AddOriginalItem<T>(object item)
        {
            ((ICollection<T>)Original).Add((T)item);
        }

        private void RemoveOriginalItem<T>(object item)
        {
            ((ICollection<T>)Original).Remove((T)item);
        }

        public override void Reset()
        {
            if (!originalItemsLoaded)
            {
                // No nested deltas possible when original items has not been loaded
                trackedItems.Clear();
            }
            else
            {
                // Only keep delta proxies, but reset them
                trackedItems = TrackedItems.Where(x => x is Delta).ToList();
                foreach (var item in trackedItems.OfType<Delta>())
                {
                    item.Reset();
                }
            }
            base.Reset();
        }
    }

    public class ObjectDelta : Delta
    {
        private Dictionary<string, object> trackedProperties = new Dictionary<string, object>();

        protected ObjectDelta()
        {
        }

        public ObjectDelta(object original, IMappedType type, ITypeMapper typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }

        protected IEnumerable<KeyValuePair<string, object>> ModifiedProperties
        {
            get
            {
                return TrackedProperties.Where(x =>
                    {
                        var delta = x.Value as Delta;
                        return delta == null || delta.IsDirty;
                    });
            }
        }

        protected Dictionary<string, object> TrackedProperties
        {
            get { return trackedProperties; }
        }

        public object GetPropertyValue(string propertyName)
        {
            object value;
            if (trackedProperties.TryGetValue(propertyName, out value))
                return value;

            IPropertyInfo prop;
            if (Type.TryGetPropertyByName(propertyName, out prop))
            {
                var propValue = prop.Getter(Original);
                if (propValue == null)
                    return null;

                var propValueType = TypeMapper.GetClassMapping(propValue.GetType());
                if (propValueType.SerializationMode != TypeSerializationMode.Value)
                {
                    var nestedDelta = CreateNestedDelta(propValue, propValueType);
                    TrackedProperties[propertyName] = nestedDelta;
                    return nestedDelta;
                }
                return propValue;
            }
            throw new KeyNotFoundException("No property with name " + propertyName + " found.");
        }


        public void SetPropertyValue(string propertyName, object value)
        {
            object oldValue;
            if (TrackedProperties.TryGetValue(propertyName, out oldValue))
            {
                DetachFromParent(oldValue);
            }
            trackedProperties[propertyName] = value;

            SetDirty();
        }


        public override void Reset()
        {
            if (!IsDirty)
                return;

            // Only keep nested deltas, and reset these
            trackedProperties = TrackedProperties.Where(x => x.Value is Delta).ToDictionary(x => x.Key, x => x.Value);
            foreach (var nestedDelta in trackedProperties.Values.Cast<Delta>())
            {
                nestedDelta.Reset();
            }
            base.Reset();
        }

        public override void Apply()
        {
            var propLookup = Type.Properties.ToLookup(x => x.Name);
            foreach (var kvp in ModifiedProperties)
            {
                var delta = kvp.Value as Delta;
                if (delta != null)
                {
                    delta.Apply();
                }
                else
                {
                    var propInfo = propLookup[kvp.Key].First();
                    propInfo.Setter(Original, kvp.Value);
                }
            }
            if (Parent == null)
                Reset();
        }
    }

    public abstract class Delta
    {
        private bool isDirty;
        private Delta parent;

        protected Delta()
        {
        }

        protected Delta(object original, IMappedType type, ITypeMapper typeMapper, Delta parent = null)
        {
            if (original == null) throw new ArgumentNullException("original");
            if (type == null) throw new ArgumentNullException("type");
            if (typeMapper == null) throw new ArgumentNullException("typeMapper");
            Original = original;
            Type = type;
            TypeMapper = typeMapper;
            this.parent = parent;
        }

        public object Original { get; internal set; }

        public IMappedType Type { get; internal set; }

        public Delta Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        public ITypeMapper TypeMapper { get; internal set; }

        protected static bool ValueIsDirty(object o)
        {
            var delta = o as IDelta;
            return delta == null || delta.IsDirty;
        }

        public bool IsDirty
        {
            get { return isDirty; }
        }

        protected static void DetachFromParent(object oldValue)
        {
            var oldDeltaValue = oldValue as Delta;
            if (oldDeltaValue != null)
                oldDeltaValue.Parent = null;
        }

        protected virtual object CreateNestedDelta(object propValue, IMappedType propValueType)
        {
            if (propValueType.SerializationMode == TypeSerializationMode.Complex)
            {
                return new ObjectDelta(propValue, propValueType, TypeMapper, this);
            }
            if (propValueType.IsCollection)
            {
                return new CollectionDelta(propValue, propValueType, TypeMapper, this);
            }
            throw new NotImplementedException();
        }

        public virtual void SetDirty()
        {
            if (isDirty)
                return;

            isDirty = true;
            if (parent != null)
            {
                parent.SetDirty();
            }
        }

        public virtual void Reset()
        {
            isDirty = false;
        }

        public abstract void Apply();
    }
}