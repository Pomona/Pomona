#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class ObjectDelta : Delta
    {
        protected ObjectDelta()
        {
        }


        public ObjectDelta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
            : base(original, type, typeMapper, parent)
        {
        }


        public IEnumerable<KeyValuePair<string, object>> ModifiedProperties
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

        protected Dictionary<string, object> TrackedProperties { get; private set; } = new Dictionary<string, object>();


        public override void Apply()
        {
            var propLookup = Type.Properties.ToLookup(x => x.Name);
            foreach (var kvp in ModifiedProperties)
            {
                var delta = kvp.Value as Delta;
                if (delta != null)
                    delta.Apply();
                else
                {
                    var propInfo = propLookup[kvp.Key].First();
                    propInfo.SetValue(Original, kvp.Value);
                }
            }
            if (Parent == null)
                Reset();
        }


        public object GetPropertyValue(string propertyName)
        {
            object value;
            if (TrackedProperties.TryGetValue(propertyName, out value))
                return value;

            PropertySpec prop;
            if (TryGetPropertyByName(propertyName, out prop))
            {
                var propValue = prop.GetValue(Original);
                if (propValue == null)
                    return null;

                var propValueType = TypeMapper.FromType(propValue.GetType());
                if (propValueType.SerializationMode != TypeSerializationMode.Value)
                {
                    var nestedDelta = CreateNestedDelta(propValue, propValueType, prop.PropertyType);
                    TrackedProperties[propertyName] = nestedDelta;
                    return nestedDelta;
                }
                return propValue;
            }
            throw new KeyNotFoundException("No property with name " + propertyName + " found.");
        }


        public override void Reset()
        {
            if (!IsDirty)
                return;

            // Only keep nested deltas, and reset these
            TrackedProperties = TrackedProperties.Where(x => x.Value is Delta).ToDictionary(x => x.Key, x => x.Value);
            foreach (var nestedDelta in TrackedProperties.Values.Cast<Delta>())
                nestedDelta.Reset();
            base.Reset();
        }


        public void SetPropertyValue(string propertyName, object value)
        {
            object trackedValue;
            if (TrackedProperties.TryGetValue(propertyName, out trackedValue))
                DetachFromParent(trackedValue);
            PropertySpec prop;
            if (TryGetPropertyByName(propertyName, out prop) && prop.PropertyType.SerializationMode == TypeSerializationMode.Value)
            {
                object oldValue = prop.GetValue(Original);
                if ((value != null && value.Equals(oldValue)) || (value == null && oldValue == null))
                    TrackedProperties.Remove(propertyName);
                else
                    TrackedProperties[propertyName] = value;
            }
            else
                TrackedProperties[propertyName] = value;

            SetDirty();
        }


        private bool TryGetPropertyByName(string propertyName, out PropertySpec prop)
        {
            return Type.TryGetPropertyByName(propertyName, StringComparison.InvariantCulture, out prop);
        }
    }
}