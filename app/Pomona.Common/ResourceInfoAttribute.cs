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
using System.Linq;
using System.Reflection;

namespace Pomona.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ResourceInfoAttribute : Attribute
    {
        private readonly Lazy<PropertyInfo> etagProperty;
        private readonly Lazy<PropertyInfo> idProperty;

        public ResourceInfoAttribute()
        {
            etagProperty = new Lazy<PropertyInfo>(GetPropertyWithAttribute<ResourceEtagPropertyAttribute>);
            idProperty = new Lazy<PropertyInfo>(GetPropertyWithAttribute<ResourceIdPropertyAttribute>);
        }

        public Type InterfaceType { get; set; }

        public bool IsUriBaseType
        {
            get { return UriBaseType == InterfaceType; }
        }

        public PropertyInfo EtagProperty
        {
            get { return etagProperty.Value; }
        }

        public bool HasEtagProperty
        {
            get { return etagProperty.Value != null; }
        }

        public PropertyInfo IdProperty
        {
            get { return idProperty.Value; }
        }

        public bool HasIdProperty
        {
            get { return idProperty.Value != null; }
        }

        public string JsonTypeName { get; set; }
        public Type LazyProxyType { get; set; }
        public Type PocoType { get; set; }
        public Type PostFormType { get; set; }
        public Type PatchFormType { get; set; }
        public Type UriBaseType { get; set; }
        public Type BaseType { get; set; }
        public string UrlRelativePath { get; set; }
        public bool IsValueObject { get; set; }

        private PropertyInfo GetPropertyWithAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            return InterfaceType.GetAllInheritedPropertiesFromInterface()
                                .FirstOrDefault(x => x.HasAttribute<TAttribute>(true));
        }
    }
}