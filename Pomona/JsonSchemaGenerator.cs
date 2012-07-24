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

using Newtonsoft.Json.Linq;

namespace Pomona
{
    public class JsonSchemaGenerator
    {
        private readonly PomonaSession session;


        public JsonSchemaGenerator(PomonaSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            this.session = session;
        }


        public JArray GenerateAllSchemas()
        {
            return new JArray(this.session.TypeMapper.TransformedTypes.Select(GenerateSchemaFor));
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


        private JToken GetPropertyDefinition(PropertyMapping prop)
        {
            var propType = prop.PropertyType;

            var jsonSchemaTypeName = propType.GetSchemaTypeName();

            var propDef = new JObject();
            propDef.Add("type", jsonSchemaTypeName);

            if (prop.CreateMode == PropertyMapping.PropertyCreateMode.Required)
                propDef.Add("required", true);

            if (!prop.IsWriteable)
                propDef.Add("readonly", true);

            if (jsonSchemaTypeName == "array")
            {
                // hackity hack hack attack. silly code but should work for now.
                propDef.Add(
                    "items", new JObject(new JProperty("type", propType.GenericArguments[0].GetSchemaTypeName())));
            }

            return propDef;
        }
    }
}