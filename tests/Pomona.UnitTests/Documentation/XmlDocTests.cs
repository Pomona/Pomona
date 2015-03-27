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

using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Documentation;
using Pomona.Example.Models;

namespace Pomona.UnitTests.Documentation
{
    [TestFixture]
    public class XmlDocTests
    {
        private static XmlDoc LoadXmlDoc()
        {
            var fileName = "Pomona.Example.xml";
            Assert.That(File.Exists(fileName), fileName + " does not exist");
            var serializer = new XmlSerializer(typeof (XmlDoc));
            XmlDoc xdoc;
            using (var stream = File.OpenRead(fileName))
            {
                xdoc = (XmlDoc)serializer.Deserialize(stream);
            }
            return xdoc;
        }

        [Test]
        public void DeserializesExampleXmlCorrectly()
        {
            var xdoc = LoadXmlDoc();
            Assert.That(xdoc.Assembly.Name, Is.EqualTo("Pomona.Example"));
            Assert.That(xdoc.Members.Count, Is.GreaterThan(0));
            Assert.That(xdoc.Members.Any(x => x.Summary != null));
        }


        [Test]
        public void GetSummary_ReturnsCorrectSummary()
        {
            var summary =
                LoadXmlDoc().GetSummary((PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<Critter>(x => x.Name));
            Assert.That(summary, Is.StringContaining("Name of the critter!"));
        }
    }
}