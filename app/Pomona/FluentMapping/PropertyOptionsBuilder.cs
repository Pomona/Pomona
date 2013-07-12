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
using System.Linq.Expressions;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal class PropertyOptionsBuilder<TDeclaringType, TPropertyType>
        : IPropertyOptionsBuilder<TDeclaringType, TPropertyType>
    {
        private readonly PropertyMappingOptions propertyMappingOptions;


        public PropertyOptionsBuilder(PropertyMappingOptions propertyMappingOptions)
        {
            if (propertyMappingOptions == null)
                throw new ArgumentNullException("propertyMappingConfigurator");
            this.propertyMappingOptions = propertyMappingOptions;
        }

        #region Implementation of IPropertyOptionsBuilder<TDeclaringType,TPropertyType>

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Writable()
        {
            propertyMappingOptions.CreateMode = PropertyCreateMode.Optional;
            propertyMappingOptions.AccessMode = PropertyAccessMode.ReadWrite;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithCreateMode(PropertyCreateMode createMode)
        {
            propertyMappingOptions.CreateMode = createMode;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithAccessMode(PropertyAccessMode accessMode)
        {
            propertyMappingOptions.AccessMode = accessMode;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsEtag()
        {
            propertyMappingOptions.IsEtagProperty = true;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingFormula(
            Expression<Func<TDeclaringType, TPropertyType>> formula)
        {
            propertyMappingOptions.Formula = formula;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey()
        {
            propertyMappingOptions.IsPrimaryKey = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name)
        {
            propertyMappingOptions.Name = name;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsAttributes()
        {
            propertyMappingOptions.IsAttributesProperty = true;
            return this;
        }

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingDecompiledFormula()
        {
            propertyMappingOptions.PropertyFormulaIsDecompiled = true;
            return this;
        }

        #endregion
    }
}