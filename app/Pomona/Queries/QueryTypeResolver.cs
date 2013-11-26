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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Queries
{
    public class QueryTypeResolver : IQueryTypeResolver
    {
        private static readonly Dictionary<string, Type> nativeTypes =
            TypeUtils.GetNativeTypes().ToDictionary(x => x.Name.ToLower(), x => x);

        private readonly ITypeMapper typeMapper;

        public QueryTypeResolver(ITypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }

        #region Implementation of IQueryTypeResolver

        public Expression ResolveProperty(Expression rootInstance, string propertyPath)
        {
            // TODO: Proper exception handling when type is not TransformedType [KNS]
            //var type = (TransformedType)this.typeMapper.GetClassMapping<T>();
            var type = (TransformedType) typeMapper.GetClassMapping(rootInstance.Type);
            return type.CreateExpressionForExternalPropertyPath(rootInstance, propertyPath);
        }


        public Type ResolveType(string typeName)
        {
            Type type;

            if (typeName.EndsWith("?"))
                return typeof (Nullable<>).MakeGenericType(ResolveType(typeName.Substring(0, typeName.Length - 1)));

            if (nativeTypes.TryGetValue(typeName.ToLower(), out type))
                return type;

            return typeMapper.GetClassMapping(typeName).Type;
        }

        #endregion
    }
}