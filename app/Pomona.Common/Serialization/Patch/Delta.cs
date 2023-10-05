#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public abstract class Delta
    {
        protected Delta()
        {
        }


        protected Delta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            Original = original;
            Type = type;
            TypeMapper = typeMapper;
            Parent = parent;
        }


        public bool IsDirty { get; private set; }
        public object Original { get; internal set; }
        public Delta Parent { get; internal set; }
        public TypeSpec Type { get; internal set; }
        public ITypeResolver TypeMapper { get; internal set; }
        public abstract void Apply();


        public virtual void Reset()
        {
            IsDirty = false;
        }


        public virtual void SetDirty()
        {
            if (IsDirty)
                return;

            IsDirty = true;
            if (Parent != null)
                Parent.SetDirty();
        }


        protected void ClearDirty()
        {
            IsDirty = false;
        }


        protected virtual object CreateNestedDelta(object propValue, TypeSpec propValueType, Type propertyType)
        {
            if (propValueType.SerializationMode == TypeSerializationMode.Structured)
                return new ObjectDelta(propValue, propValueType, TypeMapper, this);
            if (propValueType.IsCollection)
                return new CollectionDelta(propValue, propValueType, TypeMapper, this);
            throw new NotImplementedException();
        }


        protected static void DetachFromParent(object oldValue)
        {
            var oldDeltaValue = oldValue as Delta;
            if (oldDeltaValue != null)
                oldDeltaValue.Parent = null;
        }


        protected static bool ValueIsDirty(object o)
        {
            var delta = o as IDelta;
            return delta == null || delta.IsDirty;
        }
    }
}
