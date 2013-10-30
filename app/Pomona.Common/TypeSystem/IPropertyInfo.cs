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

namespace Pomona.Common.TypeSystem
{
    /// <summary>
    /// This is the pomona way of representing a property.
    /// 
    /// Can't use PropertyInfo directly, since the transformed types might not exist
    /// as Type in server context.
    /// </summary>
    public interface IPropertyInfo
    {
        bool AlwaysExpand { get; }
        PropertyCreateMode CreateMode { get; }
        IMappedType DeclaringType { get; }
        Func<object, object> Getter { get; }
        bool IsWriteable { get; }
        bool IsReadable { get; }
        bool IsSerialized { get; }
        string JsonName { get; }
        string LowerCaseName { get; }
        string Name { get; }
        IMappedType PropertyType { get; }
        Action<object, object> Setter { get; }
        bool IsPrimaryKey { get; }
        bool IsEtagProperty { get; }
        Expression CreateGetterExpression(Expression instance);
        HttpAccessMode AccessMode { get; }
        HttpAccessMode ItemAccessMode { get; }
    }
}