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

namespace Pomona.FluentMapping
{
    public interface IPropertyOptionsBuilder<TDeclaringType, TPropertyType>
    {
        /// <summary>
        /// Property defines the attributes of the resource.
        /// By doing this the property will have ResourceAttributesPropertyAttribute
        /// attached to it.
        /// </summary>
        /// <returns>the builder</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsAttributes();

        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingFormula(
            Expression<Func<TDeclaringType, TPropertyType>> formula);

        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey();
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name);
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingDecompiledFormula();
    }
}