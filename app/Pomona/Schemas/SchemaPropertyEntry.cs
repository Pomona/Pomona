#region License

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

#endregion

using System.Collections.Generic;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Schemas
{
    public class SchemaPropertyEntry
    {
        [JsonIgnore]
        public HttpMethod Access { get; set; }

        [JsonProperty(PropertyName = "access")]
        public string[] AccessMethodsAsArray
        {
            get
            {
                return this.Access == HttpMethod.Get ? null : Schema.HttpAccessModeToMethodsArray(this.Access);
            }
            set {
                this.Access = value == null ? HttpMethod.Get : Schema.MethodsArrayToHttpAccessMode(value);
            }
        }

        public IList<SchemaArrayItem> Items { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public bool Required { get; set; }
        public string Type { get; set; }
    }
}