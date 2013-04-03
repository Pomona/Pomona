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

using System.Collections.Generic;
using System.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Schemas
{
    // TODO: Implement serialization to and from Json schema

    public class SchemaGenerator
    {
        private readonly TypeMapper typeMapper;


        public SchemaGenerator(TypeMapper typeMapper)
        {
            this.typeMapper = typeMapper;
        }


        public Schema Generate()
        {
            var typeSchemas =
                typeMapper.SourceTypes.Select(typeMapper.GetClassMapping).OfType<TransformedType>().Select(
                    GenerateForType);
            return new Schema
                {
                    Types = typeSchemas.ToList(),
                    Version = typeMapper.Filter.ApiVersion
                };
        }

        private SchemaPropertyEntry GenerateForProperty(IPropertyInfo propertyInfo)
        {
            var propType = propertyInfo.PropertyType;

            var propEntry = new SchemaPropertyEntry
                {
                    Required = propertyInfo.CreateMode == PropertyCreateMode.Required,
                    Name = propertyInfo.Name,
                    Generated = propertyInfo.CreateMode == PropertyCreateMode.Excluded,
                    ReadOnly = !propertyInfo.IsWriteable,
                    Type = propType.GetSchemaTypeName()
                };

            if (propType.IsCollection)
            {
                propEntry.Items = new List<SchemaArrayItem>
                    {
                        new SchemaArrayItem {Type = propType.ElementType.GetSchemaTypeName()}
                    };
            }

            return propEntry;
        }


        private SchemaTypeEntry GenerateForType(TransformedType transformedType)
        {
            string extends = null;
            IEnumerable<IPropertyInfo> properties = transformedType.Properties;
            if (!transformedType.IsUriBaseType)
            {
                extends = transformedType.BaseType.Name;
                var propsOfBaseType = new HashSet<string>(transformedType.BaseType.Properties.Select(x => x.Name));
                properties = properties.Where(x => !propsOfBaseType.Contains(x.Name));
            }

            var typeName = transformedType.Name;

            var schemaTypeEntry = new SchemaTypeEntry
                {
                    Extends = extends,
                    Name = typeName,
                    Properties = properties.Select(GenerateForProperty).ToDictionary(x => x.Name, x => x)
                };

            if (!string.IsNullOrEmpty(transformedType.UriRelativePath))
                schemaTypeEntry.Uri = "/" + transformedType.UriRelativePath;

            return schemaTypeEntry;
        }
    }
}