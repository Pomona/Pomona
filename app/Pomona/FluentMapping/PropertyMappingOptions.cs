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
using System.Linq.Expressions;
using System.Reflection;

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
        }


        public HttpAccessMode AccessMode { get; internal set; }
        public HttpAccessMode AccessModeMask { get; internal set; }
        public bool? AlwaysExpanded { get; set; }

        public int? ConstructorArgIndex { get; set; }

        public PropertyCreateMode? CreateMode { get; internal set; }
        public LambdaExpression Formula { get; set; }

        public PropertyInclusionMode InclusionMode { get; internal set; }
        public bool? IsAttributesProperty { get; set; }
        public bool? IsEtagProperty { get; set; }

        public bool? IsPrimaryKey { get; set; }
        public HttpAccessMode ItemAccessMode { get; internal set; }
        public HttpAccessMode ItemAccessModeMask { get; internal set; }
        public string Name { get; set; }
        public bool? PropertyFormulaIsDecompiled { get; internal set; }

        public PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }


        internal void ClearAccessModeFlag(HttpAccessMode accessMode)
        {
            AccessMode &= ~accessMode;
            AccessModeMask |= accessMode;
        }


        internal void ClearItemAccessModeFlag(HttpAccessMode accessMode)
        {
            ItemAccessMode &= ~accessMode;
            ItemAccessModeMask |= accessMode;
        }


        internal void SetAccessModeFlag(HttpAccessMode accessMode)
        {
            AccessMode |= accessMode;
            AccessModeMask |= accessMode;
        }


        internal void SetItemAccessModeFlag(HttpAccessMode accessMode)
        {
            ItemAccessMode |= accessMode;
            ItemAccessModeMask |= accessMode;
        }
    }
}