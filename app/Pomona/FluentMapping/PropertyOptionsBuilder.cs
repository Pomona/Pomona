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

using Nancy.Extensions;

using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal class PropertyOptionsBuilder<TDeclaringType, TPropertyType>
        : IPropertyOptionsBuilder<TDeclaringType, TPropertyType>
    {
        private readonly PropertyMappingOptions options;


        public PropertyOptionsBuilder(PropertyMappingOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            this.options = options;
        }

        #region Implementation of IPropertyOptionsBuilder<TDeclaringType,TPropertyType>

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AlwaysExpanded()
        {
            this.options.AlwaysExpanded = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsAttributes()
        {
            this.options.IsAttributesProperty = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsEtag()
        {
            this.options.IsEtagProperty = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey()
        {
            this.options.IsPrimaryKey = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name)
        {
            this.options.Name = name;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ReadOnly()
        {
            this.options.CreateMode = PropertyCreateMode.Excluded;
            
            HttpAccessMode allMutatingMethods = HttpAccessMode.Patch | HttpAccessMode.Post | HttpAccessMode.Delete
                                            | HttpAccessMode.Put;

            this.options.SetAccessModeFlag(HttpAccessMode.Get);
            this.options.ClearAccessModeFlag(allMutatingMethods);
            this.options.SetItemAccessModeFlag(HttpAccessMode.Get);
            this.options.ClearItemAccessModeFlag(allMutatingMethods);
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingDecompiledFormula()
        {
            this.options.PropertyFormulaIsDecompiled = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingFormula(
            Expression<Func<TDeclaringType, TPropertyType>> formula)
        {
            if (formula == null)
                throw new ArgumentNullException("formula");
            this.options.Formula = formula;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithAccessMode(HttpAccessMode accessMode)
        {
            this.options.AccessModeMask = ~(default(HttpAccessMode));
            this.options.AccessMode = accessMode;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithCreateMode(PropertyCreateMode createMode)
        {
            this.options.CreateMode = createMode;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Writable()
        {
            this.options.CreateMode = PropertyCreateMode.Optional;
            if (typeof(TPropertyType).IsCollection())
                this.options.SetItemAccessModeFlag(HttpAccessMode.Patch | HttpAccessMode.Post | HttpAccessMode.Delete);
            else
            {
                if (this.options.PropertyInfo.CanWrite)
                    this.options.SetAccessModeFlag(HttpAccessMode.Put);
            }
            this.options.SetAccessModeFlag(HttpAccessMode.Patch | HttpAccessMode.Post);
            return this;
        }

        #endregion
    }
}