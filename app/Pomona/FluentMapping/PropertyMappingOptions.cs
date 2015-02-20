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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal class PropertyMappingOptions
    {
        private readonly PropertyInfo propertyInfo;


        public PropertyMappingOptions(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            this.propertyInfo = propertyInfo;
            InclusionMode = PropertyInclusionMode.Default;

            Name = propertyInfo.Name;
            AddedAttributes = new List<Attribute>();
        }


        public List<Attribute> AddedAttributes { get; set; }

        public PropertyCreateMode? CreateMode { get; internal set; }
        public bool? ExposedAsRepository { get; internal set; }
        public LambdaExpression Formula { get; set; }

        public PropertyInclusionMode InclusionMode { get; internal set; }
        public bool? IsAttributesProperty { get; set; }
        public bool? IsEtagProperty { get; set; }

        public bool? IsPrimaryKey { get; set; }
        public HttpMethod ItemMethod { get; internal set; }
        public HttpMethod ItemMethodMask { get; internal set; }
        public HttpMethod Method { get; internal set; }
        public HttpMethod MethodMask { get; internal set; }
        public string Name { get; set; }

        public Func<object, IContainer, object> OnGetDelegate { get; set; }

        public Action<object, object, IContainer> OnSetDelegate { get; set; }

        public PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }

        public Type PropertyType { get; set; }

        public ExpandMode? PropertyExpandMode { get; set; }


        internal void ClearAccessModeFlag(HttpMethod method)
        {
            Method &= ~method;
            MethodMask |= method;
        }


        internal void ClearItemAccessModeFlag(HttpMethod method)
        {
            ItemMethod &= ~method;
            ItemMethodMask |= method;
        }


        internal void SetAccessModeFlag(HttpMethod method)
        {
            Method |= method;
            MethodMask |= method;
        }


        internal void SetItemAccessModeFlag(HttpMethod method)
        {
            ItemMethod |= method;
            ItemMethodMask |= method;
        }
    }
}