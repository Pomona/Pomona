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

using Pomona.Common;
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

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Writable()
        {
            options.CreateMode = PropertyCreateMode.Optional;
            SetAccessModeFlag(PropertyAccessMode.IsWritable);
            return this;
        }


        private void SetAccessModeFlag(PropertyAccessMode accessMode)
        {
            this.options.AccessMode |= accessMode;
            this.options.AccessModeMask |= accessMode;
        }

        private void ClearAccessModeFlag(PropertyAccessMode accessMode)
        {
            this.options.AccessMode &= ~accessMode;
            this.options.AccessModeMask |= accessMode;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ReadOnly()
        {
            options.CreateMode = PropertyCreateMode.Excluded;
            SetAccessModeFlag(PropertyAccessMode.IsReadable);
            ClearAccessModeFlag(PropertyAccessMode.IsWritable);
            ClearAccessModeFlag(PropertyAccessMode.ItemChangeable);
            ClearAccessModeFlag(PropertyAccessMode.ItemInsertable);
            ClearAccessModeFlag(PropertyAccessMode.ItemRemovable);
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithCreateMode(PropertyCreateMode createMode)
        {
            options.CreateMode = createMode;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithAccessMode(PropertyAccessMode accessMode)
        {
            options.AccessModeMask = ~(default(PropertyAccessMode));
            options.AccessMode = accessMode;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AlwaysExpanded()
        {
            options.AlwaysExpanded = true;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsEtag()
        {
            options.IsEtagProperty = true;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingFormula(
            Expression<Func<TDeclaringType, TPropertyType>> formula)
        {
            options.Formula = formula;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey()
        {
            options.IsPrimaryKey = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name)
        {
            options.Name = name;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsAttributes()
        {
            options.IsAttributesProperty = true;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingDecompiledFormula()
        {
            options.PropertyFormulaIsDecompiled = true;
            return this;
        }

        #endregion
    }
}