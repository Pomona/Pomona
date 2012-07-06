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
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Pomona.TestModel;

namespace Pomona
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            //properties.Add(new JsonProperty()
            //{
            //    PropertyType = typeof(string),
            //    DeclaringType = type,
            //    Readable = true,
            //    PropertyName = "_url",
            //    ValueProvider = new ConstantValueProvider()
            //});
            return properties;
        }


        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.Length > 0
                       ? propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1, propertyName.Length - 1)
                       : propertyName;

            var sb = new StringBuilder();

            var first = true;

            foreach (var c in propertyName)
            {
                if (!char.IsLower(c) && !first)
                    sb.Append('-');

                sb.Append(char.ToLower(c));

                first = false;
            }

            return sb.ToString();
        }

        #region Nested type: ConstantValueProvider

        private class ConstantValueProvider : IValueProvider
        {
            public object GetValue(object target)
            {
                var entity = (EntityBase)target;
                return string.Format("http://localhost:2222/{0}/{1}", target.GetType().Name.ToLower(), entity.Id);
            }


            public void SetValue(object target, object value)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}