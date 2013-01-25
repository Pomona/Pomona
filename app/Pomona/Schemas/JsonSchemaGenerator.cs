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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Pomona.Common.TypeSystem;

namespace Pomona.Schemas
{
    // TODO: Implement serialization to and from Json schema

    public class Schema
    {
        public IList<SchemaTypeEntry> Types { get; set; }
        public string Version { get; set; }


        public Schema FromJson(string jsonString)
        {
            var serializer = GetSerializer();
            var stringReader = new StringReader(jsonString);
            var schema = (Schema)serializer.Deserialize(stringReader, typeof(Schema));
            // Fix property names (they're not serialized as this would be redundant)..
            foreach (var propKvp in schema.Types.SelectMany(x => x.Properties))
                propKvp.Value.Name = propKvp.Key;
            return schema;
        }


        public string ToJson()
        {
            var serializer = GetSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
        }


        private static JsonSerializer GetSerializer()
        {
            var serializer =
                JsonSerializer.Create(
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                    });
            serializer.Formatting = Formatting.Indented;
            return serializer;
        }
    }

    public class SchemaArrayItem
    {
        public string Type { get; set; }
    }

    public class SchemaPropertyEntry
    {
        public bool Generated { get; set; }
        public IList<SchemaArrayItem> Items { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public bool ReadOnly { get; set; }
        public string Type { get; set; }
    }

    public class SchemaTypeEntry
    {
        public string Name { get; set; }
        public IDictionary<string, SchemaPropertyEntry> Properties { get; set; }
    }

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
                this.typeMapper.SourceTypes.Select(this.typeMapper.GetClassMapping).OfType<TransformedType>().Select(
                    GenerateForType);
            return new Schema()
            {
                Types = typeSchemas.ToList(),
                Version = this.typeMapper.Filter.ApiVersion
            };
        }


        private SchemaPropertyEntry GenerateForProperty(IPropertyInfo propertyInfo)
        {
            var propType = propertyInfo.PropertyType;

            var propEntryType = propType.IsCollection ? "array" : propType.Name;
            var propEntry = new SchemaPropertyEntry()
            {
                Name = propertyInfo.Name,
                Generated = propertyInfo.CreateMode == PropertyCreateMode.Excluded,
                ReadOnly = !propertyInfo.IsWriteable,
                Type = propEntryType
            };

            if (propType.IsCollection)
            {
                propEntry.Items = new List<SchemaArrayItem>()
                { new SchemaArrayItem() { Type = propType.ElementType.Name } };
            }

            return propEntry;
        }


        private SchemaTypeEntry GenerateForType(TransformedType transformedType)
        {
            var typeName = transformedType.Name;
            var properties = transformedType.Properties.Select(GenerateForProperty);

            return new SchemaTypeEntry()
            {
                Name = typeName,
                Properties = properties.ToDictionary(x => x.Name, x => x)
            };
        }
    }

    public class JsonSchemaGenerator
    {
        private readonly TypeMapper typeMapper;


        public JsonSchemaGenerator(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }


        public JArray GenerateAllSchemas()
        {
            return new JArray(this.typeMapper.TransformedTypes.Select(GenerateSchemaFor));
        }


        public JObject GenerateSchemaFor(TransformedType type)
        {
            var schema = new JObject();
            schema.Add("name", type.Name);

            var properties = new JObject();

            foreach (var prop in type.Properties)
                properties.Add(prop.JsonName, GetPropertyDefinition(prop));

            schema.Add("properties", properties);

            return schema;
        }


        private JToken GetPropertyDefinition(IPropertyInfo prop)
        {
            var propType = prop.PropertyType;

            var jsonSchemaTypeName = propType.GetSchemaTypeName();

            var propDef = new JObject();
            propDef.Add("type", jsonSchemaTypeName);

            if (prop.CreateMode == PropertyCreateMode.Required)
                propDef.Add("required", true);
            else if (prop.CreateMode == PropertyCreateMode.Excluded)
                propDef.Add("generated", true);

            if (!prop.IsWriteable)
                propDef.Add("readonly", true);

            if (jsonSchemaTypeName == "array")
            {
                if (!propType.IsCollection)
                {
                    throw new InvalidOperationException(
                        "Property presented itself as JSON type array, but type is not a collection. WTF?");
                }

                // hackity hack hack attack. silly code but should work for now.
                propDef.Add(
                    "items", new JObject(new JProperty("type", propType.ElementType.GetSchemaTypeName())));
            }

            return propDef;
        }
    }
}