#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pomona.Common.Serialization;

namespace Pomona.Common.Proxies
{
    public class PutResourceBase : IPomonaSerializable
    {
        private Dictionary<string, bool> dirtyMap = new Dictionary<string, bool>();
        private Dictionary<string, object> propMap = new Dictionary<string, object>();


        protected TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            object value;
            if (!propMap.TryGetValue(property.Name, out value))
            {
                var propertyType = property.PropertyInfo.PropertyType;
                if (propertyType.IsGenericInstanceOf(typeof (IDictionary<,>)))
                {
                    var newDictType =
                        typeof (PostResourceDictionary<,>).MakeGenericType(propertyType.GetGenericArguments());
                    var newDict = Activator.CreateInstance(newDictType,
                                                           BindingFlags.Instance | BindingFlags.NonPublic |
                                                           BindingFlags.CreateInstance, null,
                                                           new object[] {this, property.Name}, null);
                    propMap[property.Name] = newDict;
                    return (TPropType) newDict;
                }
                if (propertyType.IsGenericInstanceOf(typeof (ICollection<>), typeof (IList<>)))
                {
                    var newListType = typeof (PostResourceList<>).MakeGenericType(propertyType.GetGenericArguments());
                    var newList = Activator.CreateInstance(newListType,
                                                           BindingFlags.Instance | BindingFlags.NonPublic |
                                                           BindingFlags.CreateInstance, null,
                                                           new object[] {this, property.Name}, null);
                    propMap[property.Name] = newList;
                    return (TPropType) newList;
                }
                throw new InvalidOperationException("Update value for " + property.Name + " has not been set");
            }

            return (TPropType) value;
        }


        protected void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            propMap[property.Name] = value;
            dirtyMap[property.Name] = true;
        }


        internal void SetDirty(string propertyName)
        {
            dirtyMap[propertyName] = true;
        }


        [Obsolete("Wil be removed once serialization refactoring is complete", true)]
        internal JObject ToJson(JsonSerializer jsonSerializer)
        {
            var jObject = new JObject();

            foreach (var kvp in propMap)
            {
                var jsonName = kvp.Key.LowercaseFirstLetter();
                var value = kvp.Value;

                var putValue = value as PutResourceBase;
                var hasResourceUri = value as IHasResourceUri;

                if (value == null)
                    jObject.Add(jsonName, null);
                else if (putValue != null)
                {
                    // Recursive put
                    jObject.Add(jsonName, putValue.ToJson(jsonSerializer));
                }
                else if (hasResourceUri != null)
                {
                    // Adding this as a reference
                    var propRefJObject = new JObject();
                    propRefJObject.Add("_ref", hasResourceUri.Uri);
                    jObject.Add(jsonName, propRefJObject);
                }
                else
                {
                    var valueType = value.GetType();
                    var typeCode = Type.GetTypeCode(valueType);

                    if (valueType.IsEnum || typeCode == TypeCode.Object)
                    {
                        var tokenWriter = new JTokenWriter();
                        jsonSerializer.Serialize(tokenWriter, value);
                        jObject.Add(jsonName, tokenWriter.Token);
                    }
                    else
                        jObject.Add(jsonName, new JValue(value));
                }
            }

            return jObject;
        }

        #region Implementation of IPomonaSerializable

        bool IPomonaSerializable.PropertyIsSerialized(string propertyName)
        {
            return dirtyMap.GetValueOrDefault(propertyName);
        }

        #endregion
    }
}