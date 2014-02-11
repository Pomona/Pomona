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

using System;
using System.Collections.Generic;
using System.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public class ObjectDelta : Delta
    {
        private Dictionary<string, object> trackedProperties = new Dictionary<string, object>();

        protected ObjectDelta()
        {
        }

        public ObjectDelta(object original, TypeSpec type, ITypeMapper typeMapper, Delta parent = null)
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

        protected Dictionary<string, object> TrackedProperties
        {
            get { return trackedProperties; }
        }

        public object GetPropertyValue(string propertyName)
        {
            object value;
            if (trackedProperties.TryGetValue(propertyName, out value))
                return value;

            PropertySpec prop;
            if (TryGetPropertyByName(propertyName, out prop))
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


        private bool TryGetPropertyByName(string propertyName, out PropertySpec prop)
        {
            return Type.TryGetPropertyByName(propertyName, StringComparison.InvariantCulture, out prop);
        }


        public void SetPropertyValue(string propertyName, object value)
        {
            object trackedValue;
            if (TrackedProperties.TryGetValue(propertyName, out trackedValue))
            {
                DetachFromParent(trackedValue);
            }
            PropertySpec prop;
            if (TryGetPropertyByName(propertyName, out prop) && prop.PropertyType.SerializationMode == TypeSerializationMode.Value)
            {
                object oldValue = prop.Getter(Original);
                if ((value != null && value.Equals(oldValue)) || (value == null && oldValue == null))
                {
                    trackedProperties.Remove(propertyName);
                }
                else
                {
                    trackedProperties[propertyName] = value;
                }
            }
            else
            {
                trackedProperties[propertyName] = value;
            }

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
}