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
using System.Reflection;
using Pomona.Common.Serialization;
using System.Linq;

namespace Pomona.Common.Proxies
{
    public class PutResourceBase : IPomonaSerializable
    {
        protected Dictionary<string, bool> dirtyMap = new Dictionary<string, bool>();
        protected Dictionary<string, object> propMap = new Dictionary<string, object>();


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
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
                if (typeof (IClientResource).IsAssignableFrom(propertyType))
                {
                    var resourceInfo =
                        propertyType.GetCustomAttributes(typeof (ResourceInfoAttribute), false)
                                    .OfType<ResourceInfoAttribute>()
                                    .FirstOrDefault();

                    if (resourceInfo != null && resourceInfo.IsValueObject)
                    {
                        var valueObjectForm = Activator.CreateInstance(resourceInfo.PutFormType);
                        propMap[property.Name] = valueObjectForm;
                        dirtyMap[property.Name] = true;
                        return (TPropType) valueObjectForm;
                    }
                }
                throw new InvalidOperationException("Update value for " + property.Name + " has not been set");
            }

            return (TPropType) value;
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            propMap[property.Name] = value;
            dirtyMap[property.Name] = true;
        }


        internal void SetDirty(string propertyName)
        {
            dirtyMap[propertyName] = true;
        }

        #region Implementation of IPomonaSerializable

        bool IPomonaSerializable.PropertyIsSerialized(string propertyName)
        {
            return dirtyMap.SafeGet(propertyName);
        }

        #endregion
    }
}