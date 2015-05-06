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
using System.Reflection;

using Pomona.Common.TypeSystem;

namespace Pomona.Mapping
{
    public interface ITypeConventions
    {
        #region TypeMappingConventions

        bool TypeIsMapped(Type type);
        bool TypeIsMappedAsCollection(Type type);
        bool TypeIsMappedAsSharedType(Type type);
        bool TypeIsMappedAsTransformedType(Type type);
        bool TypeIsMappedAsValueObject(Type type);

        IEnumerable<PropertyInfo> GetAllPropertiesOfType(Type type, BindingFlags bindingFlags);
        ConstructorSpec GetTypeConstructor(Type type);
        bool GetTypeIsAbstract(Type type);
        string GetTypeMappedName(Type type);
        bool IsIndependentTypeRoot(Type type);

        #endregion
    }
}