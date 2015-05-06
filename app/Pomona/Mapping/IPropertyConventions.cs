#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using Pomona.Common.TypeSystem;

namespace Pomona.Mapping
{
    public interface IPropertyConventions
    {
        IEnumerable<Attribute> GetPropertyAttributes(Type type, PropertyInfo propertyInfo);
        ExpandMode GetPropertyExpandMode(Type type, PropertyInfo propertyInfo);
        LambdaExpression GetPropertyFormula(Type type, PropertyInfo propertyInfo);
        Func<object, IContainer, object> GetPropertyGetter(Type type, PropertyInfo propertyInfo);
        string GetPropertyMappedName(Type type, PropertyInfo propertyInfo);
        Action<object, object, IContainer> GetPropertySetter(Type type, PropertyInfo propertyInfo);
        Type GetPropertyType(Type type, PropertyInfo propertyInfo);
        bool PropertyIsIncluded(Type type, PropertyInfo propertyInfo);
    }
}