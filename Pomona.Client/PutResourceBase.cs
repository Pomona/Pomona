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

using Newtonsoft.Json.Linq;

namespace Pomona.Client
{
    public class PutResourceBase
    {
        private Dictionary<string, object> putMap = new Dictionary<string, object>();


        public JObject ToJson()
        {
            var jObject = new JObject();
            foreach (var kvp in this.putMap)
            {
                var jsonName = kvp.Key.Substring(0, 1).ToLower() + kvp.Key.Substring(1);
                var value = kvp.Value;

                var putValue = value as PutResourceBase;
                var hasResourceUri = value as IHasResourceUri;

                if (putValue != null)
                {
                    // Recursive put
                    jObject.Add(jsonName, putValue.ToJson());
                }
                else if (hasResourceUri != null)
                {
                    // Adding this as a reference
                    var propRefJObject = new JObject();
                    propRefJObject.Add("_ref", hasResourceUri.Uri);
                    jObject.Add(jsonName, propRefJObject);
                }
                else
                {
                    jObject.Add(jsonName, new JValue(value));
                }
            }

            return jObject;
        }


        protected object OnPropertyGet(string propertyName)
        {
            object value;
            if (!this.putMap.TryGetValue(propertyName, out value))
                throw new InvalidOperationException("Update value for " + propertyName + " has not been set");

            return value;
        }


        protected void OnPropertySet(string propertyName, object value)
        {
            this.putMap[propertyName] = value;
        }
    }
}