#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal abstract class ExtendedProperty
    {
        protected ExtendedProperty(PropertyInfo property)
        {
            Property = property;
        }


        public PropertyInfo Property { get; }

        public abstract object GetValue(object obj, IDictionary<string, IExtendedResourceProxy> cache);
        public abstract void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache);
    }
}