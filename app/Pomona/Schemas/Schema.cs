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
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pomona.Schemas
{
    public class Schema
    {
        public IList<SchemaTypeEntry> Types { get; set; }
        public string Version { get; set; }


        public Schema FromJson(string jsonString)
        {
            var serializer = GetSerializer();
            var stringReader = new StringReader(jsonString);
            var schema = (Schema) serializer.Deserialize(stringReader, typeof (Schema));
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
                    new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                        });
            serializer.Formatting = Formatting.Indented;
            return serializer;
        }
    }
}