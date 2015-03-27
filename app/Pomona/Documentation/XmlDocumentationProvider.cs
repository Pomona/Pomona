#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Documentation
{
    public class XmlDocumentationProvider : IDocumentationProvider
    {
        private readonly Dictionary<string, XmlDoc> xmlDocs = new Dictionary<string, XmlDoc>();


        private string GetPropertyDescription(PropertyInfo property)
        {
            var xdoc = this.xmlDocs.GetOrCreate(property.DeclaringType.Assembly.FullName, () =>
            {
                var xmlDocFileName = property.ReflectedType.Assembly.GetName().Name + ".xml";
                if (File.Exists(xmlDocFileName))
                {
                    using (var stream = File.OpenRead(xmlDocFileName))
                    {
                        return (XmlDoc)(new XmlSerializer(typeof(XmlDoc)).Deserialize(stream));
                    }
                }
                return null;
            });
            if (xdoc == null)
                return null;
            return xdoc.GetSummary(property);
        }


        public string GetSummary(MemberSpec member)
        {
            var property = member as StructuredProperty;
            if (property != null)
                return GetPropertyDescription(property.PropertyInfo);
            return null;
        }
    }
}