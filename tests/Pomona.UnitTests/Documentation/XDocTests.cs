#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Documentation.Xml.Serialization;
using Pomona.Example.Models;

namespace Pomona.UnitTests.Documentation
{
    [Category("WindowsRequired")]
    [TestFixture]
    public class XDocTests
    {
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
            Assert.That(summary, Is.InstanceOf<XDocContentContainer>());
            Assert.That(summary.Count, Is.EqualTo(1));
            Assert.That(summary.First().ToString(), Contains.Substring("Name of the critter!"));
        }


        private XDoc LoadXmlDoc()
        {
            var uri = new UriBuilder(GetType().Assembly.CodeBase);
            var unescapeDataString = Uri.UnescapeDataString(uri.Path);
            var assemblyPath = Path.GetFullPath(unescapeDataString);
            var assemblyFile = new FileInfo(assemblyPath);
            var xmlFilePath = Path.Combine(assemblyFile.DirectoryName, "Pomona.Example.xml");
            Assert.That(File.Exists(xmlFilePath), xmlFilePath + " does not exist");
            XDoc xdoc;
            using (var stream = File.OpenRead(xmlFilePath))
            {
                xdoc = new XDoc(XDocument.Load(stream).Root);
            }
            return xdoc;
        }
    }
}