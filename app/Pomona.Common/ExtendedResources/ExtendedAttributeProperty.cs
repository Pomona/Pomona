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

using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal abstract class ExtendedAttributeProperty : ExtendedProperty
    {
        protected ExtendedAttributeProperty(PropertyInfo property)
            : base(property)
        {
        }


        private static ExtendedProperty Create<TDictValue, TProperty>(PropertyInfo property,
                                                                      ExtendedResourceInfo declaringTypeInfo)
        {
            return new ExtendedAttributeProperty<TDictValue, TProperty>(property, declaringTypeInfo);
        }


        public static ExtendedProperty Create(PropertyInfo property, ExtendedResourceInfo declaringTypeInfo)
        {
            return
                (ExtendedAttributeProperty)
                    createMethod.MakeGenericMethod(declaringTypeInfo.DictValueType, property.PropertyType)
                        .Invoke(null, new object[] { property, declaringTypeInfo });
        }


        private static readonly MethodInfo createMethod =
            ReflectionHelper.GetMethodDefinition(() => Create<object, object>(null, null));
    }

    internal class ExtendedAttributeProperty<TDictValue, TProperty> : ExtendedAttributeProperty
    {
        private readonly ExtendedResourceInfo declaringTypeInfo;
        private readonly string key;


        public ExtendedAttributeProperty(PropertyInfo property,
                                         ExtendedResourceInfo declaringTypeInfo,
                                         string key = null)
            : base(property)
        {
            this.declaringTypeInfo = declaringTypeInfo;
            this.key = key ?? property.Name;
        }


        private IDictionary<string, TDictValue> GetDictionary(object obj)
        {
            return (IDictionary<string, TDictValue>)declaringTypeInfo.DictProperty.GetValue(obj, null);
        }


        public override object GetValue(object obj, IDictionary<string, IExtendedResourceProxy> cache)
        {
            TDictValue value;
            if (GetDictionary(obj).TryGetValue(key, out value))
                return value;
            return null;
        }


        public override void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache)
        {
            GetDictionary(obj)[key] = (TDictValue)value;
        }
    }
}