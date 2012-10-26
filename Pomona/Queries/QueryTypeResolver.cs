#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Queries
{
    public class QueryTypeResolver : IQueryTypeResolver
    {
        private readonly TypeMapper typeMapper;


        public QueryTypeResolver(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }

        #region Implementation of IQueryTypeResolver

        public Expression Resolve<T>(Expression rootInstance, string propertyPath)
        {
            // TODO: Proper exception handling when type is not TransformedType [KNS]
            //var type = (TransformedType)this.typeMapper.GetClassMapping<T>();
            var type = (TransformedType)this.typeMapper.GetClassMapping(rootInstance.Type);
            return type.CreateExpressionForExternalPropertyPath(rootInstance, propertyPath);
        }


        public Type Resolve(string typeName)
        {
            return this.typeMapper.TransformedTypes.First(x => x.Name == typeName).SourceType;
        }

        #endregion
    }
}