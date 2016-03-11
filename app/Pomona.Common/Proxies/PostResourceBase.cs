#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Serialization;

namespace Pomona.Common.Proxies
{
    public class PostResourceBase : IPostForm
    {
        protected Dictionary<string, bool> dirtyMap = new Dictionary<string, bool>();
        protected Dictionary<string, object> propMap = new Dictionary<string, object>();


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            object value;
            if (!this.propMap.TryGetValue(property.Name, out value))
            {
                var propertyType = property.PropertyInfo.PropertyType;

                if (propertyType.IsGenericInstanceOf(typeof(IDictionary<,>)))
                {
                    var newDictType =
                        typeof(PostResourceDictionary<,>).MakeGenericType(propertyType.GetGenericArguments());
                    var newDict = Activator.CreateInstance(newDictType,
                                                           BindingFlags.Instance | BindingFlags.NonPublic |
                                                           BindingFlags.CreateInstance, null,
                                                           new object[] { this, property.Name }, null);
                    this.propMap[property.Name] = newDict;
                    return (TPropType)newDict;
                }
                if (propertyType.IsGenericInstanceOf(typeof(ISet<>)))
                {
                    var newSetType = typeof(PostResourceSet<>).MakeGenericType(propertyType.GetGenericArguments());
                    var newSet = Activator.CreateInstance(newSetType,
                                                          BindingFlags.Instance | BindingFlags.NonPublic |
                                                          BindingFlags.CreateInstance, null,
                                                          new object[] { this, property.Name }, null);
                    this.propMap[property.Name] = newSet;
                    return (TPropType)newSet;
                }
                if (propertyType.IsGenericInstanceOf(typeof(ICollection<>), typeof(IList<>)))
                {
                    var newListType = typeof(PostResourceList<>).MakeGenericType(propertyType.GetGenericArguments());
                    var newList = Activator.CreateInstance(newListType,
                                                           BindingFlags.Instance | BindingFlags.NonPublic |
                                                           BindingFlags.CreateInstance, null,
                                                           new object[] { this, property.Name }, null);
                    this.propMap[property.Name] = newList;
                    return (TPropType)newList;
                }
                if (typeof(IClientResource).IsAssignableFrom(propertyType))
                {
                    var resourceInfo =
                        propertyType.GetCustomAttributes(typeof(ResourceInfoAttribute), false)
                                    .OfType<ResourceInfoAttribute>()
                                    .FirstOrDefault();

                    if (resourceInfo != null && resourceInfo.IsValueObject)
                    {
                        var valueObjectForm = Activator.CreateInstance(resourceInfo.PostFormType);
                        this.propMap[property.Name] = valueObjectForm;
                        this.dirtyMap[property.Name] = true;
                        return (TPropType)valueObjectForm;
                    }
                }
                return default(TPropType);
            }

            return (TPropType)value;
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            this.propMap[property.Name] = value;
            this.dirtyMap[property.Name] = true;
        }


        internal void SetDirty(string propertyName)
        {
            this.dirtyMap[propertyName] = true;
        }

        #region Implementation of IPomonaSerializable

        bool IPomonaSerializable.PropertyIsSerialized(string propertyName)
        {
            return this.dirtyMap.SafeGet(propertyName);
        }

        #endregion
    }
}