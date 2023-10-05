#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

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
            var typeSchemas = this.typeMapper
                                  .SourceTypes
                                  .OfType<StructuredType>()
                                  .OrderBy(x => x.Name)
                                  .Select(GenerateForType);

            return new Schema
            {
                Types = typeSchemas.ToList(),
                Version = this.typeMapper.Filter.ApiVersion
            };
        }


        private SchemaPropertyEntry GenerateForProperty(StructuredProperty propertyInfo)
        {
            var propType = propertyInfo.PropertyType;

            var propEntry = new SchemaPropertyEntry
            {
                Required = propertyInfo.IsRequiredForConstructor,
                Access = propertyInfo.AccessMode,
                Name = propertyInfo.Name,
                Type = propType.GetSchemaTypeName()
            };

            var enumerablePropType = propType as EnumerableTypeSpec;
            if (enumerablePropType != null)
            {
                propEntry.Items = new List<SchemaArrayItem>
                {
                    new SchemaArrayItem
                    {
                        Type = enumerablePropType.ItemType.GetSchemaTypeName(),
                        Access = propertyInfo.ItemAccessMode
                    }
                };
            }

            var dictPropType = propType as DictionaryTypeSpec;
            if (dictPropType != null && propEntry.Type == "dictionary")
            {
                propEntry.Items = new List<SchemaArrayItem>()
                {
                    new SchemaArrayItem()
                    {
                        Type = dictPropType.ValueType.GetSchemaTypeName(),
                        Access = propertyInfo.ItemAccessMode
                    }
                };
            }

            return propEntry;
        }


        private SchemaTypeEntry GenerateForType(StructuredType structuredType)
        {
            string extends = null;
            var resourceTypeSpec = structuredType as ResourceType;
            IEnumerable<StructuredProperty> properties = structuredType.Properties;
            if (resourceTypeSpec == null || !resourceTypeSpec.IsUriBaseType)
            {
                if (structuredType.BaseType != null && structuredType.BaseType != typeof(object))
                {
                    extends = structuredType.BaseType.Name;
                    var propsOfBaseType = new HashSet<string>(structuredType.BaseType.Properties.Select(x => x.Name));
                    properties = properties.Where(x => !propsOfBaseType.Contains(x.Name));
                }
            }

            var typeName = structuredType.Name;

            var schemaTypeEntry = new SchemaTypeEntry
            {
                Extends = extends,
                Name = typeName,
                Properties = new SortedDictionary<string, SchemaPropertyEntry>(properties
                                                                                   .Select(GenerateForProperty)
                                                                                   .ToDictionary(x => x.Name, x => x)),
                // TODO: Expose IsAbstract on TypeSpec
                Abstract = structuredType.Type != null && structuredType.Type.IsAbstract,
                AllowedMethods = structuredType.AllowedMethods
            };

            if (resourceTypeSpec != null)
                schemaTypeEntry.Uri = resourceTypeSpec.UrlRelativePath;

            return schemaTypeEntry;
        }
    }
}

